using System.Net;

namespace ChattingApplication.Core.Interfaces;

public interface ITcpClient : IDisposable
{
  bool Connected { get; }

  Stream GetStream();
  void Close();
  Task ConnectAsync(IPEndPoint endPoint, CancellationToken token);
}
