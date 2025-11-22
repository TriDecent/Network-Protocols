
using System.Net;
using System.Net.Security;
using ChattingApplication.Core.Interfaces;

namespace ChattingApplication.Core.Sockets;

// * I don't prefer inheritance.
// In the future this will be refactored to use composition instead.

public abstract class SslTcpClientBase(ITcpClient innerClient) : ITcpClient
{
  protected readonly ITcpClient _innerClient = innerClient;
  protected SslStream? _sslStream;

  public bool Connected => _innerClient.Connected;

  public Stream GetStream()
  {
    if (_sslStream is null || !_sslStream.IsAuthenticated)
      throw new InvalidOperationException("SSL Handshake not completed.");

    return _sslStream;
  }

  public void Close() => _sslStream?.Dispose();

  public Task ConnectAsync(IPEndPoint ep, CancellationToken t) => _innerClient.ConnectAsync(ep, t);

  public abstract Task PerformHandshakeAsync(CancellationToken token);

  public void Dispose()
  {
    _sslStream?.Dispose();
    _innerClient.Dispose();
  }
}
