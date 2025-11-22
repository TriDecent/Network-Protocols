using System.Net.Security;
using ChattingApplication.Core.Interfaces;

namespace ChattingApplication.Core.Sockets;

public class ServerSslTcpClient(ITcpClient innerClient, SslServerAuthenticationOptions _options)
  : SslTcpClientBase(innerClient)
{
  public override async Task PerformHandshakeAsync(CancellationToken token)
  {
    _sslStream = new SslStream(_innerClient.GetStream(), leaveInnerStreamOpen: false);

    await _sslStream.AuthenticateAsServerAsync(_options, token);
  }
}
