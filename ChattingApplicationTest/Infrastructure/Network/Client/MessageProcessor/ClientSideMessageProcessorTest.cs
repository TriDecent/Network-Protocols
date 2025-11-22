using System.Buffers.Binary;
using System.Text.Json;
using ChattingApplication.Common.Enums;
using ChattingApplication.Core.Models;
using ChattingApplication.Core.Serializers;
using ChattingApplication.Infrastructure.Network.Client.EventEmitter;
using ChattingApplication.Infrastructure.Network.Client.MessageProcessor;
using Moq;
using NUnit.Framework;

namespace ChattingApplicationTest.Infrastructure.Network.Client.MessageProcessor;

[TestFixture]
public class ClientSideMessageProcessorTest
{
  private Mock<IMessageSerializer> _serializerMock;
  private Mock<IClientEventEmitter> _eventEmitterMock;
  private ClientSideMessageProcessor _cut;

  [SetUp]
  public void SetUp()
  {
    _serializerMock = new Mock<IMessageSerializer>();
    _eventEmitterMock = new Mock<IClientEventEmitter>();
    _cut = new ClientSideMessageProcessor(_serializerMock.Object, _eventEmitterMock.Object);
  }

  [Test]
  public async Task PrepareOutgoingMessageAsync_ShouldWriteLengthPrefixAndBody()
  {
    var message = CreateMessage(MessageType.Text, Target.All, []);
    var expectedBytes = new byte[] { 1, 2, 3, 4 };
    _serializerMock.Setup(x => x.SerializeMessageToBytesAsync(message))
      .ReturnsAsync(expectedBytes);

    var result = await _cut.PrepareOutgoingMessageAsync(message);

    Assert.That(result.Length, Is.EqualTo(4 + expectedBytes.Length));
    var prefix = BinaryPrimitives.ReadInt32BigEndian(result.Span[..4]);
    Assert.That(prefix, Is.EqualTo(expectedBytes.Length));
    Assert.That(result[4..].ToArray(), Is.EqualTo(expectedBytes));
  }

  [Test]
  public async Task HandleMessageFromStreamAsync_ShouldEmitBroadcast_WhenTargetAll()
  {
    var message = CreateMessage(MessageType.Text, Target.All, []);
    using var stream = await PrepareMessageStream(message);

    await RunHandleMessageFromStreamAsync(stream);

    _eventEmitterMock.Verify(x => x.EmitBroadcastMessageReceived(
      It.Is<Message>(m => m.Target == Target.All && m.Type == MessageType.Text)), Times.Once);
  }

  [Test]
  public async Task HandleMessageFromStreamAsync_ShouldEmitUnicast_WhenTargetIndividual()
  {
    var message = CreateMessage(MessageType.Text, Target.Individual, []);
    using var stream = await PrepareMessageStream(message);

    await RunHandleMessageFromStreamAsync(stream);

    _eventEmitterMock.Verify(x => x.EmitUnicastMessageReceived(
      It.Is<Message>(m => m.Target == Target.Individual && m.Type == MessageType.Text)), Times.Once);
  }

  [Test]
  public async Task HandleMessageFromStreamAsync_ShouldInvokeClientIdReceived_WhenCreationClientId()
  {
    string? receivedId = null;
    var processor = new ClientSideMessageProcessor(_serializerMock.Object, _eventEmitterMock.Object);
    processor.ClientIdReceived += id => receivedId = id;

    const string clientId = "abc-123";
    var message = CreateMessage(
      MessageType.CreationClientId,
      Target.Individual,
      JsonSerializer.SerializeToUtf8Bytes(clientId));
    using var stream = await PrepareMessageStream(message);

    await RunHandleMessageFromStreamAsync(stream, processor);

    Assert.That(receivedId, Is.EqualTo(clientId));
  }

  private static Message CreateMessage(MessageType type, Target target, byte[] content)
  {
    return new Message(
      new ClientInfo("1", "Test"),
      content,
      type,
      target,
      MessageRequest.None);
  }

  private static async Task<MemoryStream> PrepareMessageStream(Message message)
  {
    var messageBytes = JsonSerializer.SerializeToUtf8Bytes(message);
    var lengthBytes = new byte[4];
    BinaryPrimitives.WriteInt32BigEndian(lengthBytes, messageBytes.Length);

    var memoryStream = new MemoryStream();
    await memoryStream.WriteAsync(lengthBytes);
    await memoryStream.WriteAsync(messageBytes);
    memoryStream.Position = 0;

    return memoryStream;
  }

  private async Task RunHandleMessageFromStreamAsync(Stream stream, ClientSideMessageProcessor? processor = null)
  {
    using var cts = new CancellationTokenSource();
    try
    {
      await (processor ?? _cut).HandleMessageFromStreamAsync(stream, cts.Token);
    }
    catch (EndOfStreamException)
    {
      // Expected when stream ends
    }
  }
}