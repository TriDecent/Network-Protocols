using System.Buffers.Binary;
using System.Text.Json;
using ChattingApplication.Common.Enums;
using ChattingApplication.Core.Models;
using ChattingApplication.Core.Serializers;
using ChattingApplication.Infrastructure.Network.Server.EventEmitter;
using ChattingApplication.Infrastructure.Network.Server.MessageProcessor;
using ChattingApplication.Infrastructure.Network.Server.Operations;
using Moq;
using NUnit.Framework;

namespace ChattingApplicationTest.Infrastructure.Network.Server.MessageProcessor;

[TestFixture]
public class ServerSideMessageProcessorTest
{
  private Mock<IMessageSerializer> _serializerMock;
  private Mock<IServerEventEmitter> _eventEmitterMock;
  private Mock<IServerOperations> _serverOpsMock;
  private ServerSideMessageProcessor _cut;
  private ClientSessionInfo _sender;

  [SetUp]
  public void SetUp()
  {
    _serializerMock = new Mock<IMessageSerializer>();
    _eventEmitterMock = new Mock<IServerEventEmitter>();
    _serverOpsMock = new Mock<IServerOperations>();
    _sender = new ClientSessionInfo(new ClientInfo("1", "Test"), null!);

    _cut = new ServerSideMessageProcessor(
      _serializerMock.Object,
      _eventEmitterMock.Object
    );
    _cut.RegisterServerOperations(_serverOpsMock.Object);
  }

  [Test]
  public async Task PrepareOutgoingMessageAsync_ShouldWriteLengthPrefixAndBody()
  {
    var message = CreateMessage(MessageType.Text, Target.All);
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
  public async Task HandleMessageFromStreamAsync_ShouldCallSendClientsInfoToClientAsync_WhenGetClientsInfoRequest()
  {
    var message = CreateMessage(MessageType.Text, Target.Server, MessageRequest.GetClientsInfo);
    using var stream = await PrepareMessageStream(message);

    await RunHandleMessageFromStreamAsync(stream);

    _serverOpsMock.Verify(x => x.SendClientsInfoToClientAsync(_sender), Times.Once);
  }

  [Test]
  public async Task HandleMessageFromStreamAsync_ShouldCallSendCreationIdToSessionClientAsync_WhenGetCreationUserIdRequest()
  {
    var message = CreateMessage(MessageType.Text, Target.Server, MessageRequest.GetCreationUserId);
    using var stream = await PrepareMessageStream(message);

    await RunHandleMessageFromStreamAsync(stream);

    _serverOpsMock.Verify(x => x.SendCreationIdToSessionClientAsync(_sender), Times.Once);
  }

  [Test]
  public async Task HandleMessageFromStreamAsync_ShouldEmitReceivedUnicastMessage_WhenRecipientIsServer()
  {
    var message = CreateMessage(MessageType.Text, Target.Individual);
    var serverInfo = new ClientInfo("0", "Server");
    message = message with { Recipient = serverInfo };
    using var stream = await PrepareMessageStream(message);

    await RunHandleMessageFromStreamAsync(stream);

    _eventEmitterMock.Verify(x => x.EmitReceivedUnicastMessage(
        It.Is<Message>(m => m.Recipient == serverInfo)), Times.Once);
  }

  [Test]
  public async Task HandleMessageFromStreamAsync_ShouldForwardMessageToRecipient_WhenRecipientExists()
  {
    var recipientInfo = new ClientInfo("2", "Recipient");
    var recipientSession = new ClientSessionInfo(recipientInfo, null!);
    var message = CreateMessage(MessageType.Text, Target.Individual) with { Recipient = recipientInfo };

    _serverOpsMock.Setup(x => x.FindRecipient(recipientInfo)).Returns(recipientSession);

    using var stream = await PrepareMessageStream(message);

    await RunHandleMessageFromStreamAsync(stream);

    _serverOpsMock.Verify(x => x.ForwardMessageToClientAsync(recipientSession, message), Times.Once);
  }

  [Test]
  public async Task HandleMessageFromStreamAsync_ShouldEmitReceivedBroadcastMessage_AndBroadcast_WhenTargetAll()
  {
    var message = CreateMessage(MessageType.Text, Target.All);
    using var stream = await PrepareMessageStream(message);

    await RunHandleMessageFromStreamAsync(stream);

    _eventEmitterMock.Verify(x => x.EmitReceivedBroadcastMessage(
      It.Is<Message>(m => m.Target == Target.All)), Times.Once);
    _serverOpsMock.Verify(x => x.BroadcastMessageToClientsExceptAsync(
      It.Is<Message>(m => m.Target == Target.All), _sender), Times.Once);
  }

  private static Message CreateMessage(
    MessageType type,
    Target target,
    MessageRequest request = MessageRequest.None) =>
    new(
      new ClientInfo("1", "Test"),
      [],
      type,
      target,
      request
    );

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

  private async Task RunHandleMessageFromStreamAsync(Stream stream)
  {
    using var cts = new CancellationTokenSource();
    try
    {
      await _cut.HandleMessageFromStreamAsync(stream, _sender, cts.Token);
    }
    catch (EndOfStreamException)
    {
      // Expected when stream ends
    }
  }
}
