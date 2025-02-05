namespace ChattingApplication.Core.Models;

public interface ITcpClient
{
  Stream GetStream();
  void Close();
}
