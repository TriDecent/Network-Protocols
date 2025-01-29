using Message = ChattingApplication.Core.Models.Message;

namespace ChattingApplication.Common.Events;

public class MessageReceivedEventArgs(Message message) : EventArgs
{
  public Message Message { get; } = message;
}
