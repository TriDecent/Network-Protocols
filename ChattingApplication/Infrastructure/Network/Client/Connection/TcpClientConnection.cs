using System.Net;
using System.Net.Sockets;
using static ChattingApplication.Core.Interfaces.IClient;

namespace ChattingApplication.Infrastructure.Network.Client.Connection;

public class TcpClientConnection(
  TcpClient client) : IClientConnection
{
  private TcpClient _client = client;

  public bool IsConnected => _client.Connected;

  public async Task<ConnectionResult> ConnectAsync(
    IPEndPoint endpoint, CancellationToken token)
  {
    try
    {
      await _client.ConnectAsync(endpoint, token);
      return new ConnectionResult { Success = true };
    }
    catch (SocketException ex)
    {
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

    _client.Close();
    _client = new TcpClient();
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
