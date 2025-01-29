using ChattingApplication.Common.Enums;
using ChattingApplication.Core.Models;
using ChattingApplication.Core.Serializers;
using ChattingApplication.Infrastructure.Network.Server.Operations;
using System.Buffers.Binary;
using System.Net.Sockets;
using System.Text.Json;
using Message = ChattingApplication.Core.Models.Message;

namespace ChattingApplication.Infrastructure.Network.Server.MessageProcessor;

public class ServerSideMessageProcessor(
  IMessageSerializer serializer,
  IServerEventEmitter eventEmitter,
  IServerOperations clientOps) : IServerSideMessageProcessor
{
  private const int MESSAGE_CONTENT_SIZE_PREFIX_LENGTH = sizeof(int);
  private static readonly ClientInfo SERVER_INFO = new("0", "Server");
  private readonly IMessageSerializer _serializer = serializer;
  private readonly IServerEventEmitter _eventEmitter = eventEmitter;
  private readonly IServerOperations _clientOps = clientOps;

  public Task<Memory<byte>> PrepareOutgoingMessageAsync(Message message)
    => _serializer.SerializeMessageToBytesAsync(message);

  public async Task HandleMessageFromStreamAsync(
    NetworkStream stream,
    ClientSessionInfo sender,
    CancellationToken token)
  {
    var buffer = new byte[MESSAGE_CONTENT_SIZE_PREFIX_LENGTH];
    while (!token.IsCancellationRequested)
    {
      try
      {
        await stream.ReadExactlyAsync(buffer.AsMemory(), token);

        var contentLength = BinaryPrimitives.ReadInt32BigEndian(buffer);
        var contentBytes = new byte[contentLength];

        await stream.ReadExactlyAsync(contentBytes.AsMemory(), token);

        var message = JsonSerializer.Deserialize<Message>(contentBytes)!;

        await ProcessIncomingMessageAsync(message, sender);
      }
      catch (EndOfStreamException)
      {
        throw;
      }
    }
  }

  private async Task ProcessIncomingMessageAsync(Message message, ClientSessionInfo sender)
  {
    switch (message.Request, message.Target)
    {
      case (MessageRequest.GetClientsInfo, Target.Server):
        await _clientOps.SendClientsInfoToClientAsync(sender);
        break;

      case (MessageRequest.GetCreationUserId, Target.Server):
        await _clientOps.SendCreationIdToSessionClientAsync(sender);
        break;

      case (_, Target.Individual) when message.Recipient is not null:
        if (message.Recipient == SERVER_INFO)
        {
          _eventEmitter.EmitReceivedUnicastMessage(message);
          break;
        }

        var recipient = _clientOps.FindRecipient(message.Recipient);
        if (recipient != null)
        {
          await _clientOps.ForwardMessageToClientAsync(recipient, message);
        }
        break;

      default:
        _eventEmitter.EmitReceivedBroadcastMessage(message);
        await _clientOps.BroadcastMessageToClientsExceptAsync(message, sender);
        break;
    }
  }
}
