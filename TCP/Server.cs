using System.Net.Sockets;
using System.Text;

namespace TCP.Server;

public class Server(TcpListener listener)
{
  private readonly TcpListener _listener = listener;

  public void Start() => _listener.Start();

  public void Stop() => _listener.Stop();

  public async Task HandleMultipleClientConnectionsAsync()
  {
    while (true)
    {
      var client = await _listener.AcceptTcpClientAsync();

      _ = ProcessIncomingDataFromAsync(client);
    }
  }

  private static async Task ProcessIncomingDataFromAsync(TcpClient client)
  {
    var stream = client.GetStream();

    var buffer = new byte[1024];
    int bytesReadCount;

    while ((bytesReadCount = await stream.ReadAsync(buffer)) != 0)
    {
      var receivedData = Encoding.UTF8.GetString(buffer, 0, bytesReadCount).TrimEnd('\r', '\n');
      Console.WriteLine($"Received Data: {receivedData}");
    }
  }
}