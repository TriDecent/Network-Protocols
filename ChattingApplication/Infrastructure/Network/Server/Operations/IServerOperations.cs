using ChattingApplication.Core.Models;
using Message = ChattingApplication.Core.Models.Message;

namespace ChattingApplication.Infrastructure.Network.Server.Operations;

public interface IServerOperations
{
  Task BroadcastMessageToAllClientsAsync(Message message);
  Task BroadcastMessageToClientsExceptAsync(Message message, ClientSessionInfo excludedClient);
  Task SendUnicastMessageAsync(ClientSessionInfo recipient, Message message);
  Task SendClientsInfoToClientAsync(ClientSessionInfo recipient);
  Task SendCreationIdToSessionClientAsync(ClientSessionInfo recipient);
  Task ForwardMessageToClientAsync(ClientSessionInfo recipient, Message message);
  ClientSessionInfo? FindRecipient(ClientInfo recipientInfo);
}
