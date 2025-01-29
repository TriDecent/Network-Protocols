using ChattingApplication.Common.Enums;
using ChattingApplication.Core.Interfaces;
using ChattingApplication.Core.Models;
using ChattingApplication.Core.Serializers;
using ChattingApplication.Infrastructure.Network.Server.MessageProcessor;
using ChattingApplication.Infrastructure.Network.Server.Operations;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Message = ChattingApplication.Core.Models.Message;

namespace ChattingApplication.Infrastructure.Network.Server;

public class Server : IServer, IServerOperations
{
  private const int MESSAGE_CONTENT_SIZE_PREFIX_LENGTH = sizeof(int);
  private static readonly ClientInfo SERVER_INFO = new("0", "Server");
  private readonly TcpListener _server;
  private readonly IMessageSerializer _serializer;
  private readonly IServerEventEmitter _eventEmitter;
  private readonly IServerSideMessageProcessor _messageProcessor;
  private CancellationTokenSource _listeningCTS = new();
  private CancellationTokenSource _shutdownCTS = new();
  private readonly List<ClientSessionInfo> _clients = [];

  private bool _isListening = false;
  private bool _isRunning = false;

  public ServerState State { get; private set; } = ServerState.Shutdown;

  public IPEndPoint ServerEndPoint => (IPEndPoint)_server.LocalEndpoint;

  public IReadOnlyList<ClientSessionInfo> ClientsInfo
  {
    get
    {
      lock (_clients)
      {
        return _clients.ToList().AsReadOnly();
      }
    }
  }

  public Server(
    TcpListener server,
    IMessageSerializer serializer,
    IServerEventEmitter eventEmitter)
  {
    _server = server;
    _serializer = serializer;
    _eventEmitter = eventEmitter;

    _messageProcessor = new ServerSideMessageProcessor(
      serializer,
      eventEmitter,
      this);
  }

  public void StartListeningForConnections()
  {
    if (_isListening) return;

    UpdateState(ServerState.Starting);
    _server.Start();
    _isListening = true;
    _isRunning = true;
    UpdateState(ServerState.Listening);
  }

  public void StopListeningForConnections()
  {
    if (!_isListening) return;

    UpdateState(ServerState.Stopping);
    _server.Stop();

    _listeningCTS.Cancel();
    _listeningCTS = new();

    _isListening = false;

    UpdateState(ServerState.Stopped);
  }

  public void ShutdownAllConnections()
  {
    if (!_isRunning) return;

    UpdateState(ServerState.ShuttingDown);
    _server.Stop();
    _listeningCTS.Cancel();
    _shutdownCTS.Cancel();

    _shutdownCTS = new();
    _listeningCTS = new();

    _isListening = false;
    _isRunning = false;

    RemoveAllClients();

    UpdateState(ServerState.Shutdown);
  }

  public async Task HandleIncomingConnectionsAsync()
  {
    while (!_listeningCTS.Token.IsCancellationRequested)
    {
      TcpClient client;
      try
      {
        client = await _server.AcceptTcpClientAsync(_listeningCTS.Token);
      }
      catch (OperationCanceledException)
      {
        break;
      }

      _ = HandleClientAsync(client);
    }
  }

  public async Task BroadcastMessageToAllClientsAsync(Message message)
  {
    var messageBytes = await _serializer.SerializeMessageToBytesAsync(message);
    await BroadcastToClientsCoreAsync(messageBytes);
  }

  public async Task BroadcastMessageToClientsExceptAsync(Message message, ClientSessionInfo excludedClient)
  {
    var messageBytes = await _serializer.SerializeMessageToBytesAsync(message);
    await BroadcastToClientsCoreAsync(messageBytes, excludedClient);
  }

  public async Task SendUnicastMessageAsync(ClientSessionInfo clientInfo, Message message)
  {
    var messageBytes = await _serializer.SerializeMessageToBytesAsync(message);
    var clientStream = clientInfo.Client.GetStream();
    await clientStream.WriteAsync(messageBytes, _shutdownCTS.Token);
  }

  public Task SendClientsInfoToClientAsync(ClientSessionInfo client)
  {
    IEnumerable<ClientInfo> clientsInfo = [];
    lock (_clients)
    {
      clientsInfo = _clients.Select(client => client.Info);
    }

    var json = JsonSerializer.Serialize(clientsInfo);
    var contentBytes = Encoding.UTF8.GetBytes(json);
    var message = new Message(
      SERVER_INFO,
      contentBytes,
      MessageType.ActiveClientsInfo,
      Target.Individual,
      MessageRequest.None);

    return SendUnicastMessageAsync(client, message);
  }

  public Task SendCreationIdToSessionClientAsync(ClientSessionInfo client)
  {
    var jsonId = JsonSerializer.Serialize(client.Info.Id);
    var idBytes = Encoding.UTF8.GetBytes(jsonId);
    var message = new Message(
      SERVER_INFO,
      idBytes,
      MessageType.CreationClientId,
      Target.Individual,
      MessageRequest.None);

    return SendUnicastMessageAsync(client, message);
  }

  public Task ForwardMessageToClientAsync(ClientSessionInfo recipient, Message message)
    => SendUnicastMessageAsync(recipient, message);

  public ClientSessionInfo? FindRecipient(ClientInfo recipientInfo)
  {
    lock (_clients)
    {
      return _clients.FirstOrDefault(client => client.Info == recipientInfo);
    }
  }

  private async Task BroadcastToClientsCoreAsync(ReadOnlyMemory<byte> bytes, ClientSessionInfo? excludedClient = null)
  {
    var copiedClients = new List<ClientSessionInfo>();
    var disconnectedClients = new ConcurrentBag<ClientSessionInfo>();

    lock (_clients)
    {
      copiedClients = [.. _clients];
    }

    var broadcastTasks = copiedClients.
      Where(client => client != excludedClient).
      Select(async client =>
      {
        try
        {
          var stream = client.Client.GetStream();
          await stream.WriteAsync(bytes, _shutdownCTS.Token);
        }
        catch (IOException) // means clients got disconnected
        {
          disconnectedClients.Add(client);
        }
      });

    await Task.WhenAll(broadcastTasks);

    if (disconnectedClients.IsEmpty) return;

    foreach (var client in disconnectedClients)
    {
      RemoveClient(client);
    }
  }

  private async Task HandleClientAsync(TcpClient client)
  {
    var clientInfo = await CreateClientSessionInfoFromClient(client);
    AddClient(clientInfo);
    await HandleClientMessagesAsync(clientInfo);
  }

  private async Task<ClientSessionInfo> CreateClientSessionInfoFromClient(TcpClient client)
  {
    var message = await ReceiveSingleMessageAsync(client.GetStream());
    var clientInfo = message.Sender;
    var clientId = Guid.NewGuid().ToString();
    var newClientInfo = clientInfo with { Id = clientId };

    return new ClientSessionInfo(newClientInfo, client);
  }

  private async Task<Message> ReceiveSingleMessageAsync(NetworkStream stream)
  {
    var buffer = new byte[MESSAGE_CONTENT_SIZE_PREFIX_LENGTH];

    await stream.ReadExactlyAsync(buffer.AsMemory(), _shutdownCTS.Token);

    var contentLength = BinaryPrimitives.ReadInt32BigEndian(buffer);
    var contentBytes = new byte[contentLength];

    await stream.ReadExactlyAsync(contentBytes.AsMemory(), _shutdownCTS.Token);

    var message = JsonSerializer.Deserialize<Message>(contentBytes)!;

    return message;
  }

  private async Task HandleClientMessagesAsync(ClientSessionInfo client)
  {
    try
    {
      var stream = client.Client.GetStream();
      await _messageProcessor.HandleMessageFromStreamAsync(stream, client, _shutdownCTS.Token);
    }
    catch (EndOfStreamException)
    {
      RemoveClient(client);
    }
  }

  private void AddClient(ClientSessionInfo client)
  {
    lock (_clients)
    {
      _clients.Add(client);
      _eventEmitter.EmitChangedClientsCount(_clients.Count);
      _eventEmitter.EmitConnectedClient(client);
    }
  }

  private void RemoveClient(ClientSessionInfo client)
  {
    lock (_clients)
    {
      _clients.Remove(client);
      client.Client.Close();
      _eventEmitter.EmitChangedClientsCount(_clients.Count);
      _eventEmitter.EmitDisconnectedClient(client);
    }
  }

  private void RemoveAllClients()
  {
    lock (_clients)
    {
      var clientsToRemove = _clients.ToList();
      _clients.Clear();

      foreach (var client in clientsToRemove)
      {
        _eventEmitter.EmitDisconnectedClient(client);
        client.Client.Close();
      }

      _eventEmitter.EmitChangedClientsCount(_clients.Count);
    }
  }

  private void UpdateState(ServerState state)
  {
    State = state;
    _eventEmitter.EmitChangedState(state);
  }

  public void Dispose()
  {
    _listeningCTS.Cancel();
    _shutdownCTS.Cancel();

    _listeningCTS.Dispose();
    _shutdownCTS.Dispose();

    lock (_clients)
    {
      foreach (var client in _clients)
      {
        client.Client.Close();
      }
      _clients.Clear();
    }

    _server.Dispose();
  }
}
