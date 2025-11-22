using System.Buffers.Binary;
using System.Text.Json;
using ChattingApplication.Common.Enums;
using ChattingApplication.Core.Models;
using ChattingApplication.Core.Serializers;
using ChattingApplication.Infrastructure.Network.Server.EventEmitter;
using ChattingApplication.Infrastructure.Network.Server.Operations;
using Message = ChattingApplication.Core.Models.Message;

namespace ChattingApplication.Infrastructure.Network.Server.MessageProcessor;

public class ServerSideMessageProcessor(
  IMessageSerializer serializer,
  IServerEventEmitter eventEmitter
) : IServerSideMessageProcessor
{
  private const int LENGTH_PREFIX_SIZE = sizeof(int);
  private static readonly ClientInfo SERVER_INFO = new("0", "Server");

  private readonly IMessageSerializer _serializer = serializer;
  private readonly IServerEventEmitter _eventEmitter = eventEmitter;
  private IServerOperations? _serverOps;

  public void RegisterServerOperations(IServerOperations serverOperations)
  {
    _serverOps = serverOperations;
  }

  public async Task<Memory<byte>> PrepareOutgoingMessageAsync(Message message)
  {
    var jsonBytes = await _serializer.SerializeMessageToBytesAsync(message);
    int bodyLength = jsonBytes.Length;

    var finalPacket = new byte[LENGTH_PREFIX_SIZE + bodyLength];

    BinaryPrimitives.WriteInt32BigEndian(finalPacket.AsSpan(0, LENGTH_PREFIX_SIZE), bodyLength);

    jsonBytes.Span.CopyTo(finalPacket.AsSpan(LENGTH_PREFIX_SIZE));

    return finalPacket;
  }

  public async Task HandleMessageFromStreamAsync(
    Stream stream,
    ClientSessionInfo sender,
    CancellationToken token)
  {
    try
    {
      while (!token.IsCancellationRequested)
      {
        var message = await ReadAMessageFromStreamAsync(stream, token);
        await ProcessIncomingMessageAsync(message, sender);
      }
    }
    catch (EndOfStreamException)
    {
      throw;
    }
  }

  public async Task<Message> ReadAMessageFromStreamAsync(Stream stream, CancellationToken token)
  {
    var lengthBuffer = new byte[LENGTH_PREFIX_SIZE];

    await stream.ReadExactlyAsync(lengthBuffer.AsMemory(), token);
    var contentLength = BinaryPrimitives.ReadInt32BigEndian(lengthBuffer);

    var contentBytes = new byte[contentLength];
    await stream.ReadExactlyAsync(contentBytes.AsMemory(), token);

    return JsonSerializer.Deserialize<Message>(contentBytes)!;
  }

  private async Task ProcessIncomingMessageAsync(Message message, ClientSessionInfo sender)
  {
    if (_serverOps is null)
      throw new InvalidOperationException("ServerOperations not set in Processor!");

    switch (message.Request, message.Target)
    {
      case (MessageRequest.GetClientsInfo, Target.Server):
        await _serverOps.SendClientsInfoToClientAsync(sender);
        break;

      case (MessageRequest.GetCreationUserId, Target.Server):
        await _serverOps.SendCreationIdToSessionClientAsync(sender);
        break;

      case (_, Target.Individual) when message.Recipient is not null:
        if (message.Recipient == SERVER_INFO)
        {
          _eventEmitter.EmitReceivedUnicastMessage(message);
          break;
        }

        var recipient = _serverOps.FindRecipient(message.Recipient.Value);
        if (recipient != null)
        {
          await _serverOps.ForwardMessageToClientAsync(recipient, message);
        }
        break;

      default:
        _eventEmitter.EmitReceivedBroadcastMessage(message);
        await _serverOps.BroadcastMessageToClientsExceptAsync(message, sender);
        break;
    }
  }
}