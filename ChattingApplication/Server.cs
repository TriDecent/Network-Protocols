using System.Net.Sockets;
using System.Text;
using ChattingApplication.Enums;
using ChattingApplication.Events;
using ChattingApplication.Utils;

namespace ChattingApplication;

internal class Server(TcpListener server)
{
  private readonly TcpListener _server = server;
  private CancellationTokenSource _listeningCTS = new();
  private CancellationTokenSource _shutdownCTS = new();
  private readonly List<TcpClient> _clients = [];
  private bool _isListening = false;

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
    UpdateState(ServerState.Listening);
  }

  // TODO: make two versions of this, stop and clear all data about clients
  // version two: only stop for listening but still keep the data about clients
  public void StopListeningForConnections()
  {
    if (!_isListening) return;

    UpdateState(ServerState.Stopping);
    _server.Stop();
    _listeningCTS.Cancel();
    _isListening = false;
    UpdateState(ServerState.Stopped);
  }

  // public void TerminateAllConnections()
  // {
  //   if (!_isListening) return;

  //   UpdateState(ServerState.ShuttingDown);
  //   _server.Stop();
  //   _listeningCTS.Cancel();
  //   _isListening = false;

  //   List<TcpClient> copiedClients = [.. _clients];

  //   lock (_clients)
  //   {
  //     foreach (var client in copiedClients)
  //     {
  //       client.Close();
  //     }

  //     _clients.Clear();
  //   }
  //   UpdateState(ServerState.Stopped);
  // }

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

  public async Task BroadcastImageToAllClients(Image image)
  {
    var bytes = ImageByteConverter.ImageToBytes(image);

    await BroadcastToAllClients(bytes);
  }

  private async Task BroadcastToAllClients(byte[] bytes)
  {
    List<TcpClient> copiedClients;

    lock (_clients)
    {
      copiedClients = [.. _clients];
    }

    foreach (var client in copiedClients)
    {
      var stream = client.GetStream();
      await stream.WriteAsync(bytes.AsMemory(0, bytes.Length),
        _shutdownCTS.Token);
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
}
