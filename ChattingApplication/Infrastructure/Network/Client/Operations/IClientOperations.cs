using Message = ChattingApplication.Core.Models.Message;

namespace ChattingApplication.Infrastructure.Network.Client.Operations;

public interface IClientOperations
{
  Task SendMessageAsync(Message message);
  void UpdateName(string newName);
}
