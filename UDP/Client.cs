using System.Net;
using System.Net.Sockets;
using System.Text;

public class Client(UdpClient client)
{
  private readonly UdpClient _client = client;

  public void Connect(IPEndPoint ipEndPoint) => _client.Connect(ipEndPoint);

  public void Send(string message)
  {
    var bytes = Encoding.UTF8.GetBytes(message);
    _client.Send(bytes);
  }
}