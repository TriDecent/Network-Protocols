using ChattingApplication.Core.Models;
using Message = ChattingApplication.Core.Models.Message;

namespace ChattingApplication.Infrastructure.Network.Server.MessageProcessor;

public interface IServerSideMessageProcessor
{
  Task<Memory<byte>> PrepareOutgoingMessageAsync(Message message);
  Task HandleMessageFromStreamAsync(Stream stream, ClientSessionInfo sender, CancellationToken token);
  Task<Message> ReadAMessageFromStreamAsync(Stream stream, CancellationToken token);
}
