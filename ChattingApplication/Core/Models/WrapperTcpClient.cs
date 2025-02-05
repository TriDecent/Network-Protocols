using System.Net.Sockets;

namespace ChattingApplication.Core.Models;

public class WrapperTcpClient(TcpClient tcpClient) : ITcpClient
{
  private readonly TcpClient _client = tcpClient;

  public Stream GetStream() => _client.GetStream();
  public void Close() => _client.Close();
}
