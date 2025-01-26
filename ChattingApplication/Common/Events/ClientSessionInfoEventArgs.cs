using ChattingApplication.Core.Models;

namespace ChattingApplication.Common.Events;

public class ClientSessionInfoEventArgs(ClientSessionInfo clientInfo) : EventArgs
{
  public ClientSessionInfo ClientSessionInfo { get; } = clientInfo;
}
