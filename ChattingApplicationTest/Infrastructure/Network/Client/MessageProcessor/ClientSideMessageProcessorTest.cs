
using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Text;
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
  private string _updatedClientId;
  private ClientSideMessageProcessor _cut;

  [SetUp]
  public void SetUp()
  {
    _serializerMock = new Mock<IMessageSerializer>();
    _eventEmitterMock = new Mock<IClientEventEmitter>();
    _updatedClientId = string.Empty;
    _cut = new ClientSideMessageProcessor(
      _serializerMock.Object,
      _eventEmitterMock.Object,
      clientId => _updatedClientId = clientId);
  }

  [Test]
  public async Task PrepareOutgoingMessageAsync_ShouldSerializeMessage()
  {
    var message = CreateMessage(MessageType.Text, Target.All, []);
    var expectedBytes = new byte[] { 1, 2, 3 };
    _serializerMock.Setup(x => x.SerializeMessageToBytesAsync(message))
      .ReturnsAsync(expectedBytes);

    var result = await _cut.PrepareOutgoingMessageAsync(message);

    Assert.That(result.ToArray(), Is.EqualTo(expectedBytes));
  }

  [Test]
  public async Task HandleMessageFromStream_ShouldEmitBroadcastEvent_WhenMessageTargetIsAll()
  {
    // Arrange
    var message = CreateMessage(MessageType.Text, Target.All, []);
    using var messageStream = await PrepareMessageStream(message);

    // Act
    await ProcessMessageStream(messageStream);

    // Assert 
    _eventEmitterMock.Verify(emitter => emitter.EmitBroadcastMessageReceived(
      It.Is<Message>(message =>
        message.Target == Target.All &&
        message.Type == MessageType.Text)),
      Times.Once);
  }

  [Test]
  public async Task HandleMessageFromStream_ShouldUpdateClientId_WhenReceivedCreationIdMessage()
  {
    // Arrange
    const string clientId = "test-client-id";
    var message = CreateMessage(
      MessageType.CreationClientId,
      Target.Individual,
      JsonSerializer.SerializeToUtf8Bytes(clientId));
    using var messageStream = await PrepareMessageStream(message);

    // Act
    await ProcessMessageStream(messageStream);

    // Assert
    Assert.That(_updatedClientId, Is.EqualTo(clientId));
  }

  [Test]
  public async Task HandleMessageFromStream_ShouldEmitUnicastEvent_WhenMessageTargetIsIndividual()
  {
    // Arrange
    var message = CreateMessage(MessageType.Text, Target.Individual, []);
    using var messageStream = await PrepareMessageStream(message);

    // Act
    await ProcessMessageStream(messageStream);

    // Assert
    _eventEmitterMock.Verify(emitter => emitter.EmitUnicastMessageReceived(
      It.Is<Message>(message =>
        message.Target == Target.Individual &&
        message.Type == MessageType.Text)),
      Times.Once);
  }

  private static Message CreateMessage(
     MessageType type,
     Target target,
     byte[] content)
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

  private async Task ProcessMessageStream(Stream messageStream)
  {
    using var cts = new CancellationTokenSource();
    try
    {
      await _cut.HandleMessageFromStreamAsync(messageStream, cts.Token);
    }
    catch (EndOfStreamException)
    {
      // Expected - stream has ended
    }
  }
}
