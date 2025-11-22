using System.Text.Json;

namespace ChattingApplication.Core.Serializers;

public class MessageSerializer : IMessageSerializer
{
  public Task<Memory<byte>> SerializeMessageToBytesAsync(Models.Message message)
  {
    var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(message);

    return Task.FromResult<Memory<byte>>(jsonBytes);
  }
}