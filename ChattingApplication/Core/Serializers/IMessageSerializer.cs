namespace ChattingApplication.Core.Serializers;

public interface IMessageSerializer
{
  Task<Memory<byte>> SerializeMessageToBytesAsync(Models.Message message);
}
