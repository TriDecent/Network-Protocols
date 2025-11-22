using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using ChattingApplication.Core.Interfaces;

namespace ChattingApplication.Core.Sockets;

public class ClientSslTcpClient(ITcpClient innerClient, SslClientAuthenticationOptions _options)
  : SslTcpClientBase(innerClient)
{
  public override async Task PerformHandshakeAsync(CancellationToken token)
  {
    _sslStream = new SslStream(_innerClient.GetStream(), leaveInnerStreamOpen: false, userCertificateValidationCallback: ValidateServerCertificate);

    await _sslStream.AuthenticateAsClientAsync(_options, token);
  }

  private bool ValidateServerCertificate(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
  {
    if (sslPolicyErrors is SslPolicyErrors.None) return true;

    if (sslPolicyErrors is SslPolicyErrors.RemoteCertificateChainErrors) return true;

    return false;
  }
}
