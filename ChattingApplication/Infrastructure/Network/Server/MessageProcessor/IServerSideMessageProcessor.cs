using ChattingApplication.Core.Models;
using System.Net.Sockets;
using Message = ChattingApplication.Core.Models.Message;

namespace ChattingApplication.Infrastructure.Network.Server.MessageProcessor;

public interface IServerSideMessageProcessor
{
  Task<Memory<byte>> PrepareOutgoingMessageAsync(Message message);
  Task HandleMessageFromStreamAsync(NetworkStream stream, ClientSessionInfo sender, CancellationToken token);
}
