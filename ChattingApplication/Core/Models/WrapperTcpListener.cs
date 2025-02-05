using System.Net;
using System.Net.Sockets;
using ChattingApplication.Core.Interfaces;

namespace ChattingApplication.Core.Models;

public class WrapperTcpListener(TcpListener listener) : ITcpListener
{
  private readonly TcpListener _listener = listener;

  public EndPoint LocalEndpoint => _listener.LocalEndpoint;

  public void Start() => _listener.Start();
  public void Stop() => _listener.Stop();
  public async Task<ITcpClient> AcceptTcpClientAsync(CancellationToken token)
    => new WrapperTcpClient(await _listener.AcceptTcpClientAsync(token));
  public void Dispose() => _listener.Dispose();

}
