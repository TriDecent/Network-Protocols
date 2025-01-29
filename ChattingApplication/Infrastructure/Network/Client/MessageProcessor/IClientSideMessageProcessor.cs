using System.Net.Sockets;

namespace ChattingApplication.Infrastructure.Network.Client.MessageProcessor;

public interface IClientSideMessageProcessor
{
  void ProcessMessage(Core.Models.Message message);
  Task<Memory<byte>> PrepareOutgoingMessageAsync(Core.Models.Message message);
  Task HandleMessageFromStreamAsync(NetworkStream stream, CancellationToken token);
}
