using ChattingApplication.Enums;

namespace ChattingApplication.Events;

public class MessageReceivedEventArgs(
  Models.Message message, MessageType type) : EventArgs
{
  public Models.Message Message { get; } = message;
  public MessageType Type { get; } = type;
}
