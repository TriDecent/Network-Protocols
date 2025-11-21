using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace SecureTCP.Client;

public class SecureClient(TcpClient client)
{
  private readonly TcpClient _client = client;
  private SslStream? _sslStream;

  public async Task ConnectAsync(IPEndPoint iPEndPoint, SslClientAuthenticationOptions options)
  {
    await _client.ConnectAsync(iPEndPoint);

    _sslStream = new SslStream(_client.GetStream(), leaveInnerStreamOpen: false, userCertificateValidationCallback: ValidateServerCertificate);

    await _sslStream.AuthenticateAsClientAsync(options);
  }

  public static async Task SendMessageToStreamAsync(Stream stream, string message)
  {
    var bytes = Encoding.UTF8.GetBytes(message);
    await stream.WriteAsync(bytes);
  }

  public async Task HandleResponseFromServer()
  {
    if (_sslStream is null) return;

    int byteReadCount;
    var buffer = new byte[1024];
    while ((byteReadCount = await _sslStream.ReadAsync(buffer.AsMemory(0, buffer.Length))) > 0)
    {
      var receivedData = Encoding.UTF8.GetString(buffer, 0, byteReadCount).TrimEnd('\r', '\n');
      Console.WriteLine($"From server: {receivedData}");
    }
  }

  private bool ValidateServerCertificate(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
  {
    if (sslPolicyErrors is SslPolicyErrors.None) return true;

    if (sslPolicyErrors is SslPolicyErrors.RemoteCertificateChainErrors) return true;

    return false;
  }
}
