using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using ChattingApplication.Common.Enums;
using ChattingApplication.Common.Events;
using ChattingApplication.Core.Interfaces;
using ChattingApplication.Core.Models;
using static ChattingApplication.Core.Interfaces.IClient;

namespace ChattingApplication.Infrastructure.Network;

public class Client(TcpClient tcpClient, ClientInfo clientDetails) : IClient
{
  private TcpClient _client = tcpClient;
  private ClientInfo _clientDetails = clientDetails;
  private CancellationTokenSource _cts = new();
  public ClientInfo ClientDetails { get => _clientDetails; }
  public ClientState State { get; private set; } = ClientState.Disconnected;
  public EventHandler<StateChangedEventArgs>? StatusChangedEventHandler { get; set; }
  public EventHandler<MessageReceivedEventArgs>? MessageReceivedEventHandler { get; set; }

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

  public void UpdateName(string newName) => _clientDetails = _clientDetails with { Name = newName };

  public async Task SendMessageAsync(Core.Models.Message message)
  {
    var jsonMessage = JsonSerializer.Serialize(message);
    var bytes = Encoding.UTF8.GetBytes(jsonMessage);

    await SendBytesAsync(bytes);
  }

  private async Task SendBytesAsync(byte[] bytes)
  {
    var stream = _client.GetStream();
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

      var message = JsonSerializer.Deserialize<Core.Models.Message>(messageBytes)!;

      RaiseReceivedMessage(message);

      memoryStream.SetLength(0);
    }
  }

  private void RaiseReceivedMessage(Core.Models.Message message)
    => MessageReceivedEventHandler?.Invoke(
      this, new MessageReceivedEventArgs(message, message.Type));

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

  public void Dispose()
  {
    _cts.Cancel();
    _cts.Dispose();
    _client.Dispose();
  }
}