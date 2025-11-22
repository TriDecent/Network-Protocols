using System.Net;
using System.Net.Security;
using ChattingApplication.Core.Interfaces;
using ChattingApplication.Core.Sockets;
using static ChattingApplication.Core.Interfaces.IClient;

namespace ChattingApplication.Infrastructure.Network.Client.Connection;

public class SslClientConnectionDecorator(
  IClientConnection innerConnection, SslClientAuthenticationOptions options) : IClientConnection
{
  private readonly IClientConnection _innerConnection = innerConnection;
  private readonly SslClientAuthenticationOptions _options = options;
  private ClientSslTcpClient? _secureClientWrapper;

  public bool IsConnected => _innerConnection.IsConnected;
  public ITcpClient TcpClient => _secureClientWrapper ?? _innerConnection.TcpClient;

  public async Task<ConnectionResult> ConnectAsync(IPEndPoint endpoint, CancellationToken token)
  {
    var result = await _innerConnection.ConnectAsync(endpoint, token);

    if (!result.Success) return result;

    try
    {
      _secureClientWrapper = new ClientSslTcpClient(_innerConnection.TcpClient, _options);

      using var handshakeCts = CancellationTokenSource.CreateLinkedTokenSource(token);
      handshakeCts.CancelAfter(TimeSpan.FromSeconds(10));

      await _secureClientWrapper.PerformHandshakeAsync(handshakeCts.Token);

      return new ConnectionResult { Success = true };
    }
    catch (Exception ex)
    {
      Console.WriteLine($"SSL Handshake failed: {ex.Message}");

      Disconnect();
      return new ConnectionResult { Success = false, ErrorMessage = $"SSL Error: {ex.Message}" };
    }
  }

  public Stream GetStream()
  {
    if (_secureClientWrapper is null)
      throw new InvalidOperationException("SSL Connection not established.");

    return _secureClientWrapper.GetStream();
  }

  public async Task SendBytesAsync(ReadOnlyMemory<byte> data, CancellationToken token)
  {
    if (_secureClientWrapper is null)
      throw new InvalidOperationException("SSL Connection not established.");

    await _secureClientWrapper.GetStream().WriteAsync(data, token);
  }

  public void Disconnect()
  {
    _innerConnection.Disconnect();
    _secureClientWrapper?.Dispose();
    _secureClientWrapper = null;
  }

  public void Dispose()
  {
    _secureClientWrapper?.Dispose();
    _innerConnection.Dispose();
  }
}
