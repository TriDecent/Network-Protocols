namespace ChattingApplication;

internal class MessageReceivedEventArgs(
  object content, MessageType type) : EventArgs
{
  public object Content { get; } = content;
  public MessageType Type { get; } = type;
}
