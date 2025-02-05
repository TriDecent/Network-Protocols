using System.Net;
using System.Net.Sockets;
using ChattingApplication.Core.Interfaces;

namespace ChattingApplication.Core.Models;

public class WrapperTcpClient(TcpClient tcpClient) : ITcpClient
{
  private readonly TcpClient _client = tcpClient;

  public bool Connected => _client.Connected;

  public Stream GetStream() => _client.GetStream();
  public async Task ConnectAsync(IPEndPoint endPoint, CancellationToken token)
    => await _client.ConnectAsync(endPoint, token);
  public void Close() => _client.Close();
  public void Dispose() => _client.Dispose();
}
