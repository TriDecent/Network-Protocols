using System.Collections.Concurrent;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using SecureTCP;

var certificate = CertificateHelper.GenerateSelfSignedCertificate("test-server", "123456789");

var server = new SecureServer(new TcpListener(IPAddress.Any, 1211), certificate);

while (true)
{
  server.Start();

  await server.HandleMultipleClientConnectionsAsync();
}

public class SecureServer(TcpListener listener, X509Certificate2 serverCertificate)
{
  private readonly TcpListener _listener = listener;
  private readonly X509Certificate2 _certificate = serverCertificate;
  private readonly ConcurrentDictionary<TcpClient, SslStream> _clients = [];

  public void Start() => _listener.Start();

  public void Stop() => _listener.Stop();

  public async Task HandleMultipleClientConnectionsAsync()
  {
    _ = Task.Run(BroadCastMessageAsync);

    await HandleClients();
  }

  private void BroadCastMessageAsync()
  {
    while (true)
    {
      var message = GetStringFromServer();

      foreach (var client in _clients)
      {
        _ = SendMessageToStreamAsync(client.Value, message);
      }
    }
  }

  private async Task HandleClients()
  {
    while (true)
    {
      var client = await _listener.AcceptTcpClientAsync();

      _ = HandleClientHandShakeAsync(client);
    }
  }

  private async Task HandleClientHandShakeAsync(TcpClient client)
  {
    var sslStream = new SslStream(client.GetStream(), leaveInnerStreamOpen: false);

    try
    {
      Console.WriteLine($"[Server] Client {client.Client.RemoteEndPoint} connected. Handshaking...");


      using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

      await sslStream.AuthenticateAsServerAsync(new SslServerAuthenticationOptions
      {
        ServerCertificate = _certificate,
        ClientCertificateRequired = false,
        EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13
      }, cts.Token);

      Console.WriteLine($"[Server] Handshake Success! Cipher: {sslStream.CipherAlgorithm}");

      if (_clients.TryAdd(client, sslStream))
      {
        await ProcessIncomingDataFromStreamAsync(sslStream);
      }
    }
    catch (AuthenticationException authEx)
    {
      Console.WriteLine($"[Server] Handshake Failed: {authEx.Message}. (Is this Netcat or a Browser?)");
      Console.WriteLine(authEx);
    }
    catch (OperationCanceledException)
    {
      Console.WriteLine($"[Server] Handshake Timeout. Client too slow.");
    }
    catch (Exception ex)
    {
      Console.WriteLine($"[Server] Error: {ex.Message}");
    }
    finally
    {
      _clients.TryRemove(client, out _);
      await sslStream.DisposeAsync();
      Console.WriteLine($"[Server] Client disconnected.");
    }
  }

  private static async Task ProcessIncomingDataFromStreamAsync(Stream stream)
  {
    var buffer = new byte[1024];
    int bytesReadCount;

    while ((bytesReadCount = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length))) != 0)
    {
      var receivedData = Encoding.UTF8.GetString(buffer, 0, bytesReadCount).TrimEnd('\r', '\n');
      Console.WriteLine($"Received Data: {receivedData}");
    }
  }

  private static async Task SendMessageToStreamAsync(Stream stream, string message)
  {
    var bytes = Encoding.UTF8.GetBytes(message + Environment.NewLine);
    await stream.WriteAsync(bytes);
  }

  private static string GetStringFromServer() => Console.ReadLine()!;
}
