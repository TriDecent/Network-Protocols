using System.Net;
using System.Net.Sockets;

namespace ChattingApplication.Infrastructure.Network.Server.Connection;

public interface IServerConnection : IDisposable
{
  bool IsListening { get; }
  bool IsRunning { get; }
  IPEndPoint LocalEndPoint { get; }

  void StartListening();
  void StopListening();
  void ShutDown();
  Task<TcpClient> AcceptClientAsync(CancellationToken token);
}
