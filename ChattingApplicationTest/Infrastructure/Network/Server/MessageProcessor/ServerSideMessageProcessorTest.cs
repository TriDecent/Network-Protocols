using ChattingApplication.Common.Enums;
using ChattingApplication.Core.Models;
using ChattingApplication.Core.Serializers;
using ChattingApplication.Infrastructure.Network.Server.EventEmitter;
using ChattingApplication.Infrastructure.Network.Server.MessageProcessor;
using ChattingApplication.Infrastructure.Network.Server.Operations;
using Moq;
using NUnit.Framework;
using System.Buffers.Binary;
using System.Text.Json;

namespace ChattingApplicationTest.Infrastructure.Network.Server.MessageProcessor;

[TestFixture]
public class ServerSideMessageProcessorTest
{
  private Mock<IMessageSerializer> _serializerMock;
  private Mock<IServerEventEmitter> _eventEmitterMock;
  private Mock<IServerOperations> _clientOpsMock;
  private ServerSideMessageProcessor _cut;
  private ClientSessionInfo _sender;

  [SetUp]
  public void SetUp()
  {
    _serializerMock = new Mock<IMessageSerializer>();
    _eventEmitterMock = new Mock<IServerEventEmitter>();
    _clientOpsMock = new Mock<IServerOperations>();
    _sender = new ClientSessionInfo(new ClientInfo("1", "Test"), null!);

    _cut = new ServerSideMessageProcessor(
      _serializerMock.Object,
      _eventEmitterMock.Object,
      _clientOpsMock.Object);
  }

  [Test]
  public async Task PrepareOutgoingMessageAsync_ShouldSerializeMessage()
  {
    var message = CreateMessage(MessageType.Text, Target.All);
    var expectedBytes = new byte[] { 1, 2, 3 };
    _serializerMock
      .Setup(mock => mock.SerializeMessageToBytesAsync(message))
      .ReturnsAsync(expectedBytes);

    var result = await _cut.PrepareOutgoingMessageAsync(message);

    Assert.That(result.ToArray(), Is.EqualTo(expectedBytes));
  }

  [Test]
  public async Task HandleMessageFromStream_ShouldProcessGetClientsInfoRequest()
  {
    var message = CreateMessage(
      MessageType.Text,
      Target.Server,
      MessageRequest.GetClientsInfo);
    using var stream = await PrepareMessageStream(message);

    await ProcessMessageStream(stream);

    _clientOpsMock.Verify(mock =>
      mock.SendClientsInfoToClientAsync(_sender),
      Times.Once);
  }

  [Test]
  public async Task HandleMessageFromStream_ShouldProcessGetCreationUserIdRequest()
  {
    var message = CreateMessage(
      MessageType.Text,
      Target.Server,
      MessageRequest.GetCreationUserId);
    using var stream = await PrepareMessageStream(message);

    await ProcessMessageStream(stream);

    _clientOpsMock.Verify(x =>
      x.SendCreationIdToSessionClientAsync(_sender), Times.Once);
  }

  [Test]
  public async Task HandleMessageFromStream_ShouldEmitUnicastMessage_WhenServerIsRecipient()
  {
    var message = CreateMessage(MessageType.Text, Target.Individual);
    message = message with { Recipient = new ClientInfo("0", "Server") };
    using var stream = await PrepareMessageStream(message);

    await ProcessMessageStream(stream);

    _eventEmitterMock.Verify(x => x.EmitReceivedUnicastMessage(message), Times.Once);
  }

  [Test]
  public async Task HandleMessageFromStream_ShouldEmitAndBroadcastMessage_WhenMessageTargetIsAll()
  {
    var message = CreateMessage(MessageType.Text, Target.All);
    using var stream = await PrepareMessageStream(message);


    await ProcessMessageStream(stream);


    _eventEmitterMock.Verify(
      mock => mock.EmitReceivedBroadcastMessage(
        It.Is<Message>(message =>
          message.Type == MessageType.Text &&
          message.Target == Target.All)),
      Times.Once);

    _clientOpsMock.Verify(
      mock => mock.BroadcastMessageToClientsExceptAsync(
        It.Is<Message>(message =>
          message.Type == MessageType.Text &&
          message.Target == Target.All),
        _sender),
      Times.Once);
  }

  [Test]
  public async Task HandleMessageFromStream_ShouldForwardMessageToRecipient()
  {
    var recipient = new ClientSessionInfo(new ClientInfo("2", "Recipient"), null!);
    var message = CreateMessage(MessageType.Text, Target.Individual);
    message = message with { Recipient = recipient.Info };

    _clientOpsMock.Setup(x => x.FindRecipient(recipient.Info))
      .Returns(recipient);

    using var stream = await PrepareMessageStream(message);


    await ProcessMessageStream(stream);


    _clientOpsMock.Verify(mock =>
      mock.ForwardMessageToClientAsync(recipient, message), Times.Once);
  }

  [Test]
  public async Task HandleMessageFromStream_ShouldBroadcastMessage_WhenTargetIsAll()
  {
    var message = CreateMessage(MessageType.Text, Target.All);
    using var stream = await PrepareMessageStream(message);

    await ProcessMessageStream(stream);

    _eventEmitterMock.Verify(mock => mock.EmitReceivedBroadcastMessage(message), Times.Once);
    _clientOpsMock.Verify(x => x.BroadcastMessageToClientsExceptAsync(message, _sender), Times.Once);
  }

  private static Message CreateMessage(
    MessageType type,
    Target target,
    MessageRequest request = MessageRequest.None) => new(
      new ClientInfo("1", "Test"),
      [],
      type,
      target,
      request);

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
      await _cut.HandleMessageFromStreamAsync(messageStream, _sender, cts.Token);
    }
    catch (EndOfStreamException)
    {
      // Expected - stream has ended
    }
  }
}
