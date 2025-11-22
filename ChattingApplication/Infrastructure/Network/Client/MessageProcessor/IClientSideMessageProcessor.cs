using Message = ChattingApplication.Core.Models.Message;

namespace ChattingApplication.Infrastructure.Network.Client.MessageProcessor;

public interface IClientSideMessageProcessor
{
  Task<Memory<byte>> PrepareOutgoingMessageAsync(Message message);
  Task HandleMessageFromStreamAsync(Stream stream, CancellationToken token);

  event Action<string>? ClientIdReceived;
}
