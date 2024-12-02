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
}