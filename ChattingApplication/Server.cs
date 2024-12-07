using System.Net.Sockets;
using System.Text;
using ChattingApplication.Enums;
using ChattingApplication.Events;
using ChattingApplication.Utils;

namespace ChattingApplication;

internal class Server(TcpListener server)
{
  private readonly TcpListener _server = server;
  private readonly CancellationTokenSource _cts = new();
  private bool _isRunning = false;
  public ServerState State { get; private set; } = ServerState.Stopped;

  public EventHandler<MessageReceivedEventArgs>? MessageReceivedEventHandler;
  public EventHandler<StateChangedEventArgs>? StateChangedEventHandler;

  public void Start()
  {
    if (_isRunning) return;

    UpdateState(ServerState.Starting);
    _server.Start();
    _isRunning = true;
    UpdateState(ServerState.Listening);
  }

  public void Stop()
  {
    if (!_isRunning) return;

    UpdateState(ServerState.ShuttingDown);
    _server.Stop();
    UpdateState(ServerState.Stopped);
  }

  private void UpdateState(ServerState state)
  {
    State = state;
    RaiseChangedState();
  }

  private void RaiseChangedState()
    => StateChangedEventHandler?.Invoke(this, new StateChangedEventArgs(State, null));

  public async Task HandleMultipleConnections()
  {
    while (true)
    {
      var client = await _server.AcceptTcpClientAsync();

      _ = HandleClient(client);
    }
  }

  private async Task HandleClient(TcpClient client)
  {
    await HandleClientMessagesAsync(client);
  }

  private async Task HandleClientMessagesAsync(TcpClient client)
  {
    const int OneMB = 1048576;
    var stream = client.GetStream();

    var memoryStream = new MemoryStream();
    while (!_cts.Token.IsCancellationRequested)
    {
      var buffer = new byte[OneMB];
      var bytesReadCount = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), _cts.Token);

      await memoryStream.WriteAsync(buffer.AsMemory(0, bytesReadCount), _cts.Token);

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
