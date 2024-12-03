using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TCP.Client;

public class Client(TcpClient client)
{
  private readonly TcpClient _client = client;

  public void Connect(IPEndPoint iPEndPoint) => _client.Connect(iPEndPoint);

  public void SendMessage(string message)
  {
    var bytes = Encoding.UTF8.GetBytes(message);
    var stream = _client.GetStream();
    stream.Write(bytes);
  }

  public async Task HandleResponseFromServer()
  {
    var stream = _client.GetStream();

    int byteReadCount;
    var buffer = new byte[1024];
    while ((byteReadCount = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length))) > 0)
    {
      var receivedData = Encoding.UTF8.GetString(buffer, 0, byteReadCount).TrimEnd('\r', '\n');
      Console.WriteLine($"From server: {receivedData}");
    }
  }
}