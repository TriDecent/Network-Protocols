using System.Net;
using ChattingApplication.Core.Interfaces;

namespace ChattingApplication.Infrastructure.Network.Server.Connection;

public interface IServerConnection : IDisposable
{
  bool IsListening { get; }
  bool IsRunning { get; }
  IPEndPoint LocalEndPoint { get; }

  void StartListening();
  void StopListening();
  void ShutDown();
  Task<ITcpClient> AcceptClientAsync(CancellationToken token);
}
