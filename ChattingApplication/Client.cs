using System.Net;
using System.Net.Sockets;
using System.Text;
using ChattingApplication.Enums;
using ChattingApplication.Events;
using ChattingApplication.Utils;

namespace ChattingApplication;

internal class Client(TcpClient client) : IDisposable
{
  private TcpClient _client = client;
  private CancellationTokenSource _cts = new();
  public ClientState State { get; private set; } = ClientState.Disconnected;
  public EventHandler<StateChangedEventArgs>? StatusChangedEventHandler;
  public EventHandler<MessageReceivedEventArgs>? MessageReceivedEventHandler;

  public async Task<ConnectionResult> ConnectServerAsync(IPEndPoint ipEndPoint)
  {
    try
    {
      UpdateState(ClientState.Connecting);
      await _client.ConnectAsync(ipEndPoint);
      UpdateState(ClientState.Connected);

      _ = HandleReceivedMessageAsync().ContinueWith(cancelledTask =>
      {
        // Do nothing, valid cancellation
      }, TaskContinuationOptions.OnlyOnCanceled);

      return new ConnectionResult { Success = true };
    }
    catch (SocketException ex)
    {
      UpdateState(ClientState.Failed);
      return new ConnectionResult
      {
        Success = false,
        ErrorMessage = GetSocketErrorMessage(ex.SocketErrorCode)
      };
    }
  }

  public void DisconnectFromServer()
  {
    if (!_client.Connected) return;

    UpdateState(ClientState.Disconnecting);

    _cts.Cancel();
    _cts = new();

    _client.Close();
    _client = new TcpClient();

    UpdateState(ClientState.Disconnected);
  }

  public async Task SendMessageAsync(string message)
  {
    var bytes = Encoding.UTF8.GetBytes(message);
    await SendToClient(_client, bytes);
  }

  public async Task SendImageAsync(Image image)
  {
    var bytes = ImageByteConverter.ImageToBytes(image);
    await SendToClient(_client, bytes);
  }

  public void Dispose()
  {
    _cts.Cancel();
    _cts.Dispose();
    _client.Dispose();
  }

  private async Task SendToClient(TcpClient client, byte[] bytes)
  {
    var stream = client.GetStream();
    try
    {
      await stream.WriteAsync(bytes, _cts.Token);
    }
    catch (IOException ex)
    {
      UpdateState(ClientState.Disconnected);
      _client = new TcpClient();

      throw new IOException("Connection to server was lost", ex);
    }
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

      bytesReadCount = await stream.ReadAsync(
        buffer.AsMemory(0, buffer.Length), _cts.Token);

      if (bytesReadCount == 0)
      {
        DisconnectFromServer();

        break;
      };

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
  }

  private void RaiseReceivedMessage((byte[] Bytes, MessageType Type) message)
  {
    object content = message.Type == MessageType.Text
      ? Encoding.UTF8.GetString(message.Bytes).TrimEnd('\r', '\n')
      : message.Bytes;

    MessageReceivedEventHandler?.Invoke(
      this, new MessageReceivedEventArgs(content, message.Type));
  }

  private static string GetSocketErrorMessage(SocketError errorCode) => errorCode switch
  {
    SocketError.ConnectionRefused =>
        "Could not connect to the server. Please try again later.",
    SocketError.TimedOut =>
        "Connection attempt timed out. The server is not responding.",
    SocketError.HostUnreachable or SocketError.NetworkUnreachable =>
        "The server is not reachable. Please check your internet connection.",
    _ => "An unexpected error occurred. Please try again later."
  };

  internal class ConnectionResult
  {
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
  }
}
