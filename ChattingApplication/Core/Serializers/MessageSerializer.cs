using System.Buffers.Binary;
using System.Text;
using System.Text.Json;

namespace ChattingApplication.Core.Serializers;

public class MessageSerializer : IMessageSerializer
{
  private const int MESSAGE_CONTENT_SIZE_PREFIX_LENGTH = sizeof(int);

  public async Task<Memory<byte>> SerializeMessageToBytesAsync(Models.Message message)
  {
    var jsonMessage = JsonSerializer.Serialize(message);
    var contentLengthBytes = new byte[MESSAGE_CONTENT_SIZE_PREFIX_LENGTH];
    var contentBytes = Encoding.UTF8.GetBytes(jsonMessage);

    BinaryPrimitives.WriteInt32BigEndian(contentLengthBytes, contentBytes.Length);

    using var memoryStream = new MemoryStream();
    await memoryStream.WriteAsync(contentLengthBytes);
    await memoryStream.WriteAsync(contentBytes);

    return memoryStream.ToArray().AsMemory();
  }
}