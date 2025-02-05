using System.Net;

namespace ChattingApplication.Core.Interfaces;

public interface ITcpListener : IDisposable
{
  EndPoint LocalEndpoint { get; }
  void Start();
  void Stop();
  Task<ITcpClient> AcceptTcpClientAsync(CancellationToken token);
}
