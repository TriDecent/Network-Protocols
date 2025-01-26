using ChattingApplication.Common.Enums;
using ChattingApplication.Common.Events;
using ChattingApplication.Core.Interfaces;
using ChattingApplication.Core.Models;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace ChattingApplication.Infrastructure.Network;

public class Server(TcpListener server) : IServer
{
  private const int MESSAGE_CONTENT_SIZE_PREFIX_LENGTH = sizeof(int);
  private readonly TcpListener _server = server;
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
  public int ConnectedClientsCount { get => _clients.Count; }

  public event EventHandler<int>? ClientsCountChangedEventHandler;
  public event EventHandler<MessageReceivedEventArgs>? MessageReceivedEventHandler;
  public event EventHandler<StateChangedEventArgs>? StateChangedEventHandler;
  public event EventHandler<ClientSessionInfo>? ClientConnectedEventHandler;
  public event EventHandler<ClientSessionInfo>? ClientDisconnectedEventHandler;

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

  public async Task BroadcastMessageToAllClientsAsync(Core.Models.Message message)
  {
    var messageBytes = await SerializeMessageToBytesAsync(message);
    await BroadcastToClientsCoreAsync(messageBytes);
  }

  private async Task BroadcastMessageToClientsExceptAsync(Core.Models.Message message, ClientSessionInfo excludedClient)
  {
    var messageBytes = await SerializeMessageToBytesAsync(message);
    await BroadcastToClientsCoreAsync(messageBytes, excludedClient);
  }

  private static async Task<byte[]> SerializeMessageToBytesAsync(Core.Models.Message message)
  {
    var jsonMessage = JsonSerializer.Serialize(message);
    var contentLengthBytes = new byte[MESSAGE_CONTENT_SIZE_PREFIX_LENGTH];
    var contentBytes = Encoding.UTF8.GetBytes(jsonMessage);

    BinaryPrimitives.WriteInt32BigEndian(contentLengthBytes, contentBytes.Length);

    using var memoryStream = new MemoryStream();
    await memoryStream.WriteAsync(contentLengthBytes);
    await memoryStream.WriteAsync(contentBytes);

    return memoryStream.ToArray();
  }

  private async Task BroadcastToClientsCoreAsync(byte[] bytes, ClientSessionInfo? excludedClient = null)
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
          await stream.WriteAsync(bytes.AsMemory(0, bytes.Length), _shutdownCTS.Token);
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
    var clientInfo = await GetClientSessionInfo(client);
    AddClient(clientInfo);
    await HandleClientMessagesAsync(clientInfo);
  }

  private async Task<ClientSessionInfo> GetClientSessionInfo(TcpClient client)
  {
    var stream = client.GetStream();

    var buffer = new byte[MESSAGE_CONTENT_SIZE_PREFIX_LENGTH];

    await stream.ReadExactlyAsync(buffer.AsMemory(), _shutdownCTS.Token);

    var contentLength = BinaryPrimitives.ReadInt32BigEndian(buffer);
    var contentBytes = new byte[contentLength];

    await stream.ReadExactlyAsync(contentBytes.AsMemory(), _shutdownCTS.Token);

    var clientInfo = JsonSerializer.Deserialize<ClientInfo>(contentBytes)!;

    return new ClientSessionInfo(clientInfo, client);
  }

  private async Task HandleClientMessagesAsync(ClientSessionInfo client)
  {
    var stream = client.Client.GetStream();

    var buffer = new byte[MESSAGE_CONTENT_SIZE_PREFIX_LENGTH];
    while (!_shutdownCTS.Token.IsCancellationRequested)
    {
      try
      {
        await stream.ReadExactlyAsync(buffer.AsMemory(), _shutdownCTS.Token);

        var contentLength = BinaryPrimitives.ReadInt32BigEndian(buffer);
        var contentBytes = new byte[contentLength];

        await stream.ReadExactlyAsync(contentBytes.AsMemory(), _shutdownCTS.Token);

        var message = JsonSerializer.Deserialize<Core.Models.Message>(contentBytes)!;

        RaiseReceivedMessage(message);

        await BroadcastMessageToClientsExceptAsync(message, client);
      }
      catch (EndOfStreamException)
      {
        RemoveClient(client);
        break;
      }
    }
  }

  private void AddClient(ClientSessionInfo client)
  {
    lock (_clients)
    {
      _clients.Add(client);
      RaiseChangedClientsCount();
      RaiseConnectedClient(client);
    }
  }

  private void RemoveClient(ClientSessionInfo client)
  {
    lock (_clients)
    {
      _clients.Remove(client);
      client.Client.Close();
      RaiseChangedClientsCount();
      RaiseDisconnectedClient(client);
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
        RaiseDisconnectedClient(client);
        client.Client.Close();
      }

      RaiseChangedClientsCount();
    }
  }

  private void UpdateState(ServerState state)
  {
    State = state;
    RaiseChangedState();
  }

  private void RaiseChangedState()
    => StateChangedEventHandler?.Invoke(this, new StateChangedEventArgs(State, null));

  private void RaiseReceivedMessage(Core.Models.Message message)
    => MessageReceivedEventHandler?.Invoke(
      this, new MessageReceivedEventArgs(message, message.Type));

  private void RaiseChangedClientsCount()
    => ClientsCountChangedEventHandler?.Invoke(this, ConnectedClients);

  private void RaiseConnectedClient(ClientSessionInfo clientInfo)
    => ClientConnectedEventHandler?.Invoke(this, clientInfo);

  private void RaiseDisconnectedClient(ClientSessionInfo clientInfo)
    => ClientDisconnectedEventHandler?.Invoke(this, clientInfo);

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