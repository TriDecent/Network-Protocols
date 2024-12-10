using ChattingApplication.Common.Enums;

namespace ChattingApplication.Common.Events;

public class MessageReceivedEventArgs(
  Core.Models.Message message, MessageType type) : EventArgs
{
  public Core.Models.Message Message { get; } = message;
  public MessageType Type { get; } = type;
}
