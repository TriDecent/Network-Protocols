using System.Net;
using System.Net.Sockets;

namespace ChattingApplication.Core.Interfaces;

public interface ITcpListener : IDisposable
{
  EndPoint LocalEndpoint { get; }
  void Start();
  void Stop();
  Task<TcpClient> AcceptTcpClientAsync(CancellationToken token);
}
