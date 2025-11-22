using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Text.Json;
using ChattingApplication.Common.Enums;
using ChattingApplication.Core.Interfaces;
using ChattingApplication.Core.Models;
using ChattingApplication.Core.Serializers;
using ChattingApplication.Infrastructure.Network.Server.Connection;
using ChattingApplication.Infrastructure.Network.Server.EventEmitter;
using ChattingApplication.Infrastructure.Network.Server.MessageProcessor;
using ChattingApplication.Infrastructure.Network.Server.Operations;
using Message = ChattingApplication.Core.Models.Message;

namespace ChattingApplication.Infrastructure.Network.Server;

public class Server : IServer, IServerOperations, IDisposable
{
  private static readonly ClientInfo SERVER_INFO = new("0", "Server");
  private readonly IServerConnection _connection;
  private readonly IServerEventEmitter _eventEmitter;
  private readonly IServerSideMessageProcessor _messageProcessor;
  private CancellationTokenSource _listeningCTS = new();
  private CancellationTokenSource _shutdownCTS = new();
  private readonly List<ClientSessionInfo> _clients = [];

  public ServerState State { get; private set; } = ServerState.Shutdown;

  public IPEndPoint ServerEndPoint => _connection.LocalEndPoint;

  public IReadOnlyList<ClientSessionInfo> ClientsInfo
  {
    get
    {
      lock (_clients)
      {
        return [.. _clients];
      }
    }
  }

  public Server(
    IServerConnection connection,
    IMessageSerializer serializer,
    IServerEventEmitter eventEmitter)
  {
    _connection = connection;
    _eventEmitter = eventEmitter;

    _messageProcessor = new ServerSideMessageProcessor(
      serializer,
      eventEmitter,
      this);
  }

  public void StartListeningForConnections()
  {
    if (_connection.IsListening) return;

    UpdateState(ServerState.Starting);
    _connection.StartListening();

    UpdateState(ServerState.Listening);
  }

  public void StopListeningForConnections()
  {
    if (!_connection.IsListening) return;

    UpdateState(ServerState.Stopping);
    _connection.StopListening();

    _listeningCTS.Cancel();
    _listeningCTS = new();

    UpdateState(ServerState.Stopped);
  }

  public void ShutdownAllConnections()
  {
    if (!_connection.IsRunning) return;

    UpdateState(ServerState.ShuttingDown);
    _connection.ShutDown();

    _listeningCTS.Cancel();
    _shutdownCTS.Cancel();

    _shutdownCTS = new();
    _listeningCTS = new();

    RemoveAllClients();

    UpdateState(ServerState.Shutdown);
  }

  public async Task HandleIncomingConnectionsAsync()
  {
    while (!_listeningCTS.Token.IsCancellationRequested)
    {
      ITcpClient client;
      try
      {
        client = await _connection.AcceptClientAsync(_listeningCTS.Token);
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
    var packet = await _messageProcessor.PrepareOutgoingMessageAsync(message);
    await BroadcastToClientsCoreAsync(packet);
  }

  public async Task BroadcastMessageToClientsExceptAsync(Message message, ClientSessionInfo excludedClient)
  {
    var packet = await _messageProcessor.PrepareOutgoingMessageAsync(message);
    await BroadcastToClientsCoreAsync(packet, excludedClient);
  }

  public async Task SendUnicastMessageAsync(ClientSessionInfo clientInfo, Message message)
  {
    var packet = await _messageProcessor.PrepareOutgoingMessageAsync(message);
    var clientStream = clientInfo.Client.GetStream();
    await clientStream.WriteAsync(packet, _shutdownCTS.Token);
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

  private async Task HandleClientAsync(ITcpClient client)
  {
    var clientInfo = await CreateClientSessionInfoFromClient(client);
    AddClient(clientInfo);
    await HandleClientMessagesAsync(clientInfo);
  }

  private async Task<ClientSessionInfo> CreateClientSessionInfoFromClient(ITcpClient client)
  {
    var message = await _messageProcessor.ReadAMessageFromStreamAsync(client.GetStream(), _shutdownCTS.Token);
    var clientInfo = message.Sender;
    var clientId = Guid.NewGuid().ToString();
    var newClientInfo = clientInfo with { Id = clientId };

    return new ClientSessionInfo(newClientInfo, client);
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

    _connection.Dispose();
  }
}
