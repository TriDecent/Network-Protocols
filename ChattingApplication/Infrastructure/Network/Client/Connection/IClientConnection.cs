using System.Net;
using static ChattingApplication.Core.Interfaces.IClient;

namespace ChattingApplication.Infrastructure.Network.Client.Connection;

public interface IClientConnection : IDisposable
{
  bool IsConnected { get; }

  Task<ConnectionResult> ConnectAsync(IPEndPoint endpoint, CancellationToken token);
  void Disconnect();
  Task SendBytesAsync(ReadOnlyMemory<byte> data, CancellationToken token);
  Stream GetStream();
}
