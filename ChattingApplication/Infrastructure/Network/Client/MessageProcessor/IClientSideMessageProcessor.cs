using System.Net.Sockets;

namespace ChattingApplication.Infrastructure.Network.Client.MessageProcessor;

public interface IClientSideMessageProcessor
{
  Task<Memory<byte>> PrepareOutgoingMessageAsync(Core.Models.Message message);
  Task HandleMessageFromStreamAsync(NetworkStream stream, CancellationToken token);
}
