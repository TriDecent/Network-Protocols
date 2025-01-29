namespace ChattingApplication.Common.Events;

public class MessageReceivedEventArgs(Core.Models.Message message) : EventArgs
{
  public Core.Models.Message Message { get; } = message;
}
