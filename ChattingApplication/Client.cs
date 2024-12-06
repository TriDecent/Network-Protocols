using System.Net;
using System.Net.Sockets;
using System.Text;
using ChattingApplication.Enums;
using ChattingApplication.Utils;

namespace ChattingApplication;

internal class Client(TcpClient client)
{
  private TcpClient _client = client;
  private readonly CancellationTokenSource _cts = new();
  public ClientState State { get; private set; } = ClientState.Disconnected;
  public EventHandler<StateChangedEventArgs>? StatusChangedEventHandler;
  public EventHandler<MessageReceivedEventArgs>? MessageReceivedEventHandler;

  public async Task ConnectServerAsync(IPEndPoint ipEndPoint)
  {
    try
    {
      UpdateState(ClientState.Connecting);
      await _client.ConnectAsync(ipEndPoint);
      UpdateState(ClientState.Connected);

      await HandleReceivedMessageAsync();
    }
    catch (SocketException)
    {
      UpdateState(ClientState.Failed);
      throw;
    }
  }

  public void DisconnectFromServer()
  {
    if (!_client.Connected) return;

    UpdateState(ClientState.Disconnecting);
    _cts.Cancel();
    _client.Close();

    _client = new TcpClient();
    UpdateState(ClientState.Disconnected);
  }

  public void SendMessage(string message)
  {
    var bytes = Encoding.UTF8.GetBytes(message);
    var stream = _client.GetStream();
    stream.Write(bytes);
  }

  private void UpdateState(ClientState state)
  {
    State = state;
    RaiseChangedState();
  }

  private void RaiseChangedState()
    => StatusChangedEventHandler?.Invoke(this, new StateChangedEventArgs(null, State));

  private async Task HandleReceivedMessageAsync()
  {
    const int OneMB = 1048576;
    var stream = _client.GetStream();

    var buffer = new byte[OneMB];

    using var memoryStream = new MemoryStream();

    while (!_cts.Token.IsCancellationRequested)
    {
      int bytesReadCount;
      try
      {
        bytesReadCount = await stream.ReadAsync(
            buffer.AsMemory(0, buffer.Length), _cts.Token);

        if (bytesReadCount == 0) break;

        await memoryStream.WriteAsync(
            buffer.AsMemory(0, bytesReadCount), _cts.Token);

        if (stream.DataAvailable) continue;

        var messageBytes = memoryStream.ToArray();
        RaiseReceivedMessage((messageBytes,
            messageBytes.IsImage() ?
            MessageType.Image :
            MessageType.Text));

        memoryStream.SetLength(0);
      }
      catch (OperationCanceledException) 
      {
        break;
      }
    }
  }

  private void RaiseReceivedMessage((byte[] Bytes, MessageType Type) message)
  {
    object content = message.Type == MessageType.Text
      ? Encoding.UTF8.GetString(message.Bytes)
      : message.Bytes;

    MessageReceivedEventHandler?.Invoke(
      this, new MessageReceivedEventArgs(content, message.Type));
  }
}
