using ChattingApplication.Common.Enums;
using ChattingApplication.Core.Serializers;
using ChattingApplication.Infrastructure.Network.Client.EventEmitter;
using System.Buffers.Binary;
using System.Text.Json;
using Message = ChattingApplication.Core.Models.Message;

namespace ChattingApplication.Infrastructure.Network.Client.MessageProcessor;

public class ClientSideMessageProcessor(
  IMessageSerializer serializer,
  IClientEventEmitter eventEmitter,
  Action<string> updateClientId) : IClientSideMessageProcessor
{
  private const int MESSAGE_CONTENT_SIZE_PREFIX_LENGTH = sizeof(int);
  private readonly IMessageSerializer _serializer = serializer;
  private readonly IClientEventEmitter _eventEmitter = eventEmitter;
  private readonly Action<string> _updateClientId = updateClientId;

  public Task<Memory<byte>> PrepareOutgoingMessageAsync(Message message)
    => _serializer.SerializeMessageToBytesAsync(message);

  public async Task HandleMessageFromStreamAsync(Stream stream, CancellationToken token)
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

        ProcessMessage(message);
      }
      catch (EndOfStreamException)
      {
        throw;
      }
    }
  }

  private void ProcessMessage(Message message)
  {
    if (message.Target is Target.All)
    {
      _eventEmitter.EmitBroadcastMessageReceived(message);
      return;
    }

    if (message.Target is Target.Individual &&
      message.Type is MessageType.CreationClientId)
    {
      var clientId = JsonSerializer.Deserialize<string>(message.Content)!;
      _updateClientId(clientId);
    }

    _eventEmitter.EmitUnicastMessageReceived(message);
  }
}