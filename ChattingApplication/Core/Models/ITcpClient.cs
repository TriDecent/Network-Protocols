using System.Net;

namespace ChattingApplication.Core.Models;

public interface ITcpClient : IDisposable
{
  bool Connected { get; }

  Stream GetStream();
  void Close();
  Task ConnectAsync(IPEndPoint endPoint, CancellationToken token);
}
