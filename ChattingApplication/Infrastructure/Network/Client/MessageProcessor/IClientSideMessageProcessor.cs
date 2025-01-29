using System.Net.Sockets;
using Message = ChattingApplication.Core.Models.Message;

namespace ChattingApplication.Infrastructure.Network.Client.MessageProcessor;

public interface IClientSideMessageProcessor
{
  Task<Memory<byte>> PrepareOutgoingMessageAsync(Message message);
  Task HandleMessageFromStreamAsync(NetworkStream stream, CancellationToken token);
}
