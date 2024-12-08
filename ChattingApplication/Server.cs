using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using ChattingApplication.Enums;
using ChattingApplication.Events;
using ChattingApplication.Utils;

namespace ChattingApplication;

internal class Server(TcpListener server) : IDisposable
{
  private readonly TcpListener _server = server;
  private CancellationTokenSource _listeningCTS = new();
  private CancellationTokenSource _shutdownCTS = new();
  private readonly List<TcpClient> _clients = [];
  private bool _isListening = false;
  private bool _isRunning = false;

  public ServerState State { get; private set; } = ServerState.Shutdown;

  public EventHandler<MessageReceivedEventArgs>? MessageReceivedEventHandler;
  public EventHandler<StateChangedEventArgs>? StateChangedEventHandler;

  public void StartListeningForConnections()
  {
    if (_isListening) return;

    _listeningCTS = new CancellationTokenSource();
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
    _isListening = false;
    _isRunning = false;

    List<TcpClient> copiedClients = [.. _clients];

    lock (_clients)
    {
      foreach (var client in copiedClients)
      {
        client.Close();
      }

      _clients.Clear();
    }

    UpdateState(ServerState.Shutdown);
  }

  private void UpdateState(ServerState state)
  {
    State = state;
    RaiseChangedState();
  }

  private void RaiseChangedState()
    => StateChangedEventHandler?.Invoke(this, new StateChangedEventArgs(State, null));

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

      lock (_clients)
      {
        _clients.Add(client);
      }

      _ = HandleClientAsync(client);
    }
  }

  public async Task BroadcastMessageToAllClients(string message)
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
  {
    var copiedClients = new List<TcpClient>();
    var disconnectedClients = new ConcurrentBag<TcpClient>();

    lock (_clients)
    {
      copiedClients = [.. _clients];
    }

    var broadcastTasks = copiedClients.Select(async client =>
    {
      try
      {
        var stream = client.GetStream();
        await stream.WriteAsync(bytes.AsMemory(0, bytes.Length), _shutdownCTS.Token);
      }
      catch (IOException) // means clients got disconnected
      {
        disconnectedClients.Add(client);
        client.Close();
      }
    });

    await Task.WhenAll(broadcastTasks);

    if (disconnectedClients.IsEmpty) return;

    lock (_clients)
    {
      foreach (var client in disconnectedClients)
      {
        _clients.Remove(client);
      }
    }
  }

  private async Task HandleClientAsync(TcpClient client)
    => await HandleClientMessagesAsync(client);

  private async Task HandleClientMessagesAsync(TcpClient client)
  {
    const int OneMB = 1048576;
    var stream = client.GetStream();

    var memoryStream = new MemoryStream();
    while (!_shutdownCTS.Token.IsCancellationRequested)
    {
      var buffer = new byte[OneMB];
      var bytesReadCount = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length),
        _shutdownCTS.Token);

      if (bytesReadCount == 0) break;

      await memoryStream.WriteAsync(buffer.AsMemory(0, bytesReadCount),
        _shutdownCTS.Token);

      if (stream.DataAvailable) continue;

      var messageBytes = memoryStream.ToArray();

      RaiseReceivedMessage((messageBytes,
        messageBytes.IsImage() ?
        MessageType.Image :
        MessageType.Text));

      memoryStream.SetLength(0);
    }
  }

  private void RaiseReceivedMessage((byte[] Bytes, MessageType Type) message)
  {
    object content = message.Type == MessageType.Text
      ? Encoding.UTF8.GetString(message.Bytes).TrimEnd('\r', '\n')
      : message.Bytes;

    MessageReceivedEventHandler?.Invoke(
      this, new MessageReceivedEventArgs(content, message.Type));
  }

  public void Dispose()
  {
    _shutdownCTS.Cancel();
    _listeningCTS.Cancel();

    _shutdownCTS.Dispose();
    _listeningCTS.Dispose();

    foreach (var client in _clients)
    {
      client.Close();
    }

    _clients.Clear();
  }
}