using ChattingApplication.Common.Enums;
using ChattingApplication.Infrastructure.Network.Client.EventEmitter;
using System.Net;
using System.Net.Sockets;
using static ChattingApplication.Core.Interfaces.IClient;

namespace ChattingApplication.Infrastructure.Network.Client.Connection;

public class TcpClientConnection(
  TcpClient client,
  IClientEventEmitter eventEmitter) : IClientConnection
{
  private TcpClient _client = client;
  private readonly IClientEventEmitter _eventEmitter = eventEmitter;

  public bool IsConnected => _client.Connected;

  public async Task<ConnectionResult> ConnectAsync(
    IPEndPoint endpoint, CancellationToken token)
  {
    try
    {
      _eventEmitter.EmitStateChanged(ClientState.Connecting);
      await _client.ConnectAsync(endpoint, token);
      _eventEmitter.EmitStateChanged(ClientState.Connected);
      return new ConnectionResult { Success = true };
    }
    catch (SocketException ex)
    {
      _eventEmitter.EmitStateChanged(ClientState.Failed);
      return new ConnectionResult
      {
        Success = false,
        ErrorMessage = GetSocketErrorMessage(ex.SocketErrorCode)
      };
    }
  }

  public void Disconnect()
  {
    if (!IsConnected) return;

    _eventEmitter.EmitStateChanged(ClientState.Disconnecting);

    _client.Close();
    _client = new TcpClient();

    _eventEmitter.EmitStateChanged(ClientState.Disconnected);
  }

  public async Task SendBytesAsync(ReadOnlyMemory<byte> data, CancellationToken token)
  {
    try
    {
      var stream = _client.GetStream();
      await stream.WriteAsync(data, token);
    }
    catch (IOException) // Connection to Server was lost
    {
      _eventEmitter.EmitStateChanged(ClientState.Disconnected);
      _client = new TcpClient();
      throw;
    }
  }

  public NetworkStream GetStream() => _client.GetStream();

  public void Dispose() => _client.Dispose();

  private static string GetSocketErrorMessage(SocketError errorCode)
  => errorCode switch
  {
    SocketError.ConnectionRefused =>
      "Could not connect to the server. Please try again later.",
    SocketError.TimedOut =>
      "Connection attempt timed out. The server is not responding.",
    SocketError.HostUnreachable or SocketError.NetworkUnreachable =>
      "The server is not reachable. Please check your internet connection.",
    _ => "An unexpected error occurred. Please try again later."
  };
}
