using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ChattingApplication.Enums;
using ChattingApplication.Events;
using ChattingApplication.Utils;

namespace ChattingApplication.Server;

public class Server(TcpListener server) : IServer
{
  private readonly TcpListener _server = server;
  private CancellationTokenSource _listeningCTS = new();
  private CancellationTokenSource _shutdownCTS = new();
  private readonly List<TcpClient> _clients = [];
  private bool _isListening = false;
  private bool _isRunning = false;

  public ServerState State { get; private set; } = ServerState.Shutdown;

  public IPEndPoint ServerEndPoint => (IPEndPoint)_server.LocalEndpoint;

  public int ConnectedClients { get => _clients.Count; }

  public EventHandler<int>? ClientsChangedEventHandler { get; set; }
  public EventHandler<MessageReceivedEventArgs>? MessageReceivedEventHandler { get; set; }
  public EventHandler<StateChangedEventArgs>? StateChangedEventHandler { get; set; }

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

      AddClient(client);

      _ = HandleClientAsync(client);
    }
  }

  public async Task BroadcastTextToAllClientsAsync(string message)
  {
    var bytes = Encoding.UTF8.GetBytes(message);

    await BroadcastToAllClients(bytes);
  }

  public async Task BroadcastImageToAllClientsAsync(Image image)
  {
    var bytes = ImageByteConverter.ImageToBytes(image);

    await BroadcastToAllClients(bytes);
  }

  private async Task BroadcastToAllClients(byte[] bytes)
    => await BroadcastToClientsCore(bytes);

  private async Task BroadcastToAllExcept(byte[] bytes, TcpClient excludedClient)
    => await BroadcastToClientsCore(bytes, excludedClient);

  private async Task BroadcastToClientsCore(byte[] bytes, TcpClient? excludedClient = null)
  {
    var copiedClients = new List<TcpClient>();
    var disconnectedClients = new ConcurrentBag<TcpClient>();

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
          var stream = client.GetStream();
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
    => await HandleClientMessagesAsync(client);

  private async Task HandleClientMessagesAsync(TcpClient client)
  {
    const int BufferSize = 1048576;
    var stream = client.GetStream();

    var memoryStream = new MemoryStream();
    while (!_shutdownCTS.Token.IsCancellationRequested)
    {
      var buffer = new byte[BufferSize];
      var bytesReadCount = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length),
        _shutdownCTS.Token);

      if (bytesReadCount == 0)
      {
        RemoveClient(client);
        break;
      };

      await memoryStream.WriteAsync(buffer.AsMemory(0, bytesReadCount),
        _shutdownCTS.Token);

      if (stream.DataAvailable) continue;

      var messageBytes = memoryStream.ToArray();

      RaiseReceivedMessage((messageBytes,
        messageBytes.IsImage() ?
        MessageType.Image :
        MessageType.Text));

      await BroadcastToAllExcept(messageBytes, client);

      memoryStream.SetLength(0);
    }
  }

  private void AddClient(TcpClient client)
  {
    lock (_clients)
    {
      _clients.Add(client);
      RaiseChangedConnectedClients();
    }
  }

  private void RemoveClient(TcpClient client)
  {
    lock (_clients)
    {
      _clients.Remove(client);
      client.Close();
      RaiseChangedConnectedClients();
    }
  }

  private void RemoveAllClients()
  {
    lock (_clients)
    {
      foreach (var client in _clients)
      {
        _clients.Remove(client);
        client.Close();
        RaiseChangedConnectedClients();
      }
    }
  }

  private void UpdateState(ServerState state)
  {
    State = state;
    RaiseChangedState();
  }

  private void RaiseChangedState()
    => StateChangedEventHandler?.Invoke(this, new StateChangedEventArgs(State, null));

  private void RaiseReceivedMessage((byte[] Bytes, MessageType Type) message)
  {
    object content = message.Type == MessageType.Text
      ? Encoding.UTF8.GetString(message.Bytes).TrimEnd('\r', '\n')
      : message.Bytes;

    MessageReceivedEventHandler?.Invoke(
      this, new MessageReceivedEventArgs(content, message.Type));
  }

  private void RaiseChangedConnectedClients()
    => ClientsChangedEventHandler?.Invoke(this, ConnectedClients);

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
        client.Close();
      }
      _clients.Clear();
    }

    _server.Dispose();
  }
}