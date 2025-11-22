using System.Buffers.Binary;
using System.Text.Json;
using ChattingApplication.Common.Enums;
using ChattingApplication.Core.Serializers;
using ChattingApplication.Infrastructure.Network.Client.EventEmitter;
using Message = ChattingApplication.Core.Models.Message;

namespace ChattingApplication.Infrastructure.Network.Client.MessageProcessor;

public class ClientSideMessageProcessor(
  IMessageSerializer serializer,
  IClientEventEmitter eventEmitter) : IClientSideMessageProcessor
{
  private const int LENGTH_PREFIX_SIZE = sizeof(int);
  private readonly IMessageSerializer _serializer = serializer;
  private readonly IClientEventEmitter _eventEmitter = eventEmitter;

  public event Action<string>? ClientIdReceived;

  public async Task<Memory<byte>> PrepareOutgoingMessageAsync(Message message)
  {
    var jsonBytes = await _serializer.SerializeMessageToBytesAsync(message);
    int bodyLength = jsonBytes.Length;
    var finalPacket = new byte[LENGTH_PREFIX_SIZE + bodyLength];
    BinaryPrimitives.WriteInt32BigEndian(finalPacket.AsSpan(0, LENGTH_PREFIX_SIZE), bodyLength);
    jsonBytes.Span.CopyTo(finalPacket.AsSpan(LENGTH_PREFIX_SIZE));
    return finalPacket;
  }

  public async Task HandleMessageFromStreamAsync(Stream stream, CancellationToken token)
  {
    while (!token.IsCancellationRequested)
    {
      var message = await ReadAMessageFromStreamAsync(stream, token);
      ProcessMessage(message);
    }
  }

  private static async Task<Message> ReadAMessageFromStreamAsync(Stream stream, CancellationToken token)
  {
    var lengthBuffer = new byte[LENGTH_PREFIX_SIZE];

    await stream.ReadExactlyAsync(lengthBuffer.AsMemory(), token);
    var contentLength = BinaryPrimitives.ReadInt32BigEndian(lengthBuffer);
    var contentBytes = new byte[contentLength];

    await stream.ReadExactlyAsync(contentBytes.AsMemory(), token);
    var message = JsonSerializer.Deserialize<Message>(contentBytes)!;

    return message;
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
      ClientIdReceived?.Invoke(clientId);
    }

    _eventEmitter.EmitUnicastMessageReceived(message);
  }
}
