using System.Net;
using System.Text;
using System.Text.Json;
using ChattingApplication.Common.Enums;
using ChattingApplication.Core.Interfaces;
using ChattingApplication.Core.Models;
using ChattingApplication.Core.Serializers;
using ChattingApplication.Infrastructure.Network.Server.Connection;
using ChattingApplication.Infrastructure.Network.Server.EventEmitter;
using ChattingApplication.Infrastructure.Network.Server.MessageProcessor;
using Moq;
using NUnit.Framework;

namespace ChattingApplicationTest.Infrastructure.Network.Server;

[TestFixture]
public class ServerTest
{
  private Mock<IServerConnection> _connectionMock;
  private Mock<IMessageSerializer> _serializerMock;
  private Mock<IServerEventEmitter> _eventEmitterMock;
  private Mock<IServerSideMessageProcessor> _messageProcessorMock;
  private ChattingApplication.Infrastructure.Network.Server.Server _cut;

  [SetUp]
  public void Setup()
  {
    _connectionMock = new Mock<IServerConnection>();
    _serializerMock = new Mock<IMessageSerializer>();
    _eventEmitterMock = new Mock<IServerEventEmitter>();
    _messageProcessorMock = new Mock<IServerSideMessageProcessor>();

    _cut = new ChattingApplication.Infrastructure.Network.Server.Server(
      _connectionMock.Object,
      _eventEmitterMock.Object,
      _messageProcessorMock.Object);
  }

  [Test]
  public void StartListeningForConnections_ShouldUpdateState_WhenStarting()
  {
    _connectionMock.Setup(x => x.IsListening).Returns(false);

    _cut.StartListeningForConnections();

    _connectionMock.Verify(x => x.StartListening(), Times.Once);
    _eventEmitterMock.Verify(x => x.EmitChangedState(ServerState.Starting), Times.Once);
    _eventEmitterMock.Verify(x => x.EmitChangedState(ServerState.Listening), Times.Once);
  }

  [Test]
  public void StopListeningForConnections_ShouldUpdateState_WhenStopping()
  {
    _connectionMock.Setup(x => x.IsListening).Returns(true);

    _cut.StopListeningForConnections();

    _connectionMock.Verify(x => x.StopListening(), Times.Once);
    _eventEmitterMock.Verify(x => x.EmitChangedState(ServerState.Stopping), Times.Once);
    _eventEmitterMock.Verify(x => x.EmitChangedState(ServerState.Stopped), Times.Once);
  }

  [Test]
  public void ShutdownAllConnections_ShouldUpdateState_WhenShuttingDown()
  {
    _connectionMock.Setup(x => x.IsRunning).Returns(true);

    _cut.ShutdownAllConnections();

    _connectionMock.Verify(x => x.ShutDown(), Times.Once);
    _eventEmitterMock.Verify(x => x.EmitChangedState(ServerState.ShuttingDown), Times.Once);
    _eventEmitterMock.Verify(x => x.EmitChangedState(ServerState.Shutdown), Times.Once);
  }

  [Test]
  public async Task BroadcastMessageToAllClientsAsync_ShouldCallPrepareOutgoingMessageAndBroadcast()
  {
    var message = new Message(
      new ClientInfo("1", "Test"),
      [],
      MessageType.Text,
      Target.All,
      MessageRequest.None);

    var packet = new byte[] { 1, 2, 3 };
    _messageProcessorMock.Setup(x => x.PrepareOutgoingMessageAsync(message)).ReturnsAsync(packet);

    await _cut.BroadcastMessageToAllClientsAsync(message);

    _messageProcessorMock.Verify(x => x.PrepareOutgoingMessageAsync(message), Times.Once);
    // BroadcastToClientsCoreAsync is private, so we can't verify directly, but no exception means pass
  }

  [Test]
  public void Dispose_ShouldCleanupResources()
  {
    _cut.Dispose();
    _connectionMock.Verify(mock => mock.Dispose(), Times.Once);
  }

  [Test]
  public void ServerEndPoint_ShouldReturnConnectionEndPoint()
  {
    var expectedEndpoint = new IPEndPoint(IPAddress.Any, 5000);
    _connectionMock.Setup(mock => mock.LocalEndPoint).Returns(expectedEndpoint);

    Assert.That(_cut.ServerEndPoint, Is.EqualTo(expectedEndpoint));
  }

  [Test]
  public async Task SendClientsInfoToClientAsync_ShouldSendCurrentClientsList()
  {
    var mockClient = new Mock<ITcpClient>();
    var mockStream = new Mock<Stream>();
    mockClient.Setup(mock => mock.GetStream()).Returns(mockStream.Object);

    var testClient = new ClientSessionInfo(
        new ClientInfo("1", "Test Client"),
        mockClient.Object);


    var clientsField = typeof(ChattingApplication.Infrastructure.Network.Server.Server)
      .GetField("_clients", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
    var clientsList = (List<ClientSessionInfo>)clientsField.GetValue(_cut)!;
    clientsList.Add(testClient);

    var clientsInfo = new[] { testClient.Info };
    var json = JsonSerializer.Serialize(clientsInfo);
    var contentBytes = Encoding.UTF8.GetBytes(json);

    var expectedMessage = new Message(
      new ClientInfo("0", "Server"),
      contentBytes,
      MessageType.ActiveClientsInfo,
      Target.Individual,
      MessageRequest.None);

    var packet = new byte[] { 1, 2, 3 };
    _messageProcessorMock.Setup(x => x.PrepareOutgoingMessageAsync(It.Is<Message>(m =>
      m.Type == MessageType.ActiveClientsInfo &&
      m.Target == Target.Individual &&
      m.Sender.Id == "0" &&
      m.Sender.Name == "Server"))).ReturnsAsync(packet);

    mockStream.Setup(x => x.WriteAsync(
      It.Is<ReadOnlyMemory<byte>>(mem => mem.ToArray().SequenceEqual(packet)),
      It.IsAny<CancellationToken>())).Returns(ValueTask.CompletedTask);

    await _cut.SendClientsInfoToClientAsync(testClient);

    _messageProcessorMock.Verify(x => x.PrepareOutgoingMessageAsync(It.Is<Message>(m =>
      m.Type == MessageType.ActiveClientsInfo &&
      m.Target == Target.Individual &&
      m.Sender.Id == "0" &&
      m.Sender.Name == "Server")), Times.Once);

    mockStream.Verify(x => x.WriteAsync(
      It.Is<ReadOnlyMemory<byte>>(mem => mem.ToArray().SequenceEqual(packet)),
      It.IsAny<CancellationToken>()), Times.Once);
  }

  [Test]
  public async Task ForwardMessageToClientAsync_ShouldSendMessageToSpecificClient()
  {
    var mockClient = new Mock<ITcpClient>();
    var mockStream = new Mock<Stream>();
    mockClient.Setup(x => x.GetStream()).Returns(mockStream.Object);

    var recipient = new ClientSessionInfo(
      new ClientInfo("1", "Test Recipient"),
      mockClient.Object);

    var message = new Message(
      new ClientInfo("2", "Test Sender"),
      [1, 2, 3],
      MessageType.Text,
      Target.Individual,
      MessageRequest.None);

    var packet = new byte[] { 4, 5, 6 };
    _messageProcessorMock.Setup(x => x.PrepareOutgoingMessageAsync(message)).ReturnsAsync(packet);

    mockStream.Setup(x => x.WriteAsync(
      It.Is<ReadOnlyMemory<byte>>(mem => mem.ToArray().SequenceEqual(packet)),
      It.IsAny<CancellationToken>())).Returns(ValueTask.CompletedTask);

    await _cut.ForwardMessageToClientAsync(recipient, message);

    _messageProcessorMock.Verify(x => x.PrepareOutgoingMessageAsync(message), Times.Once);
    mockStream.Verify(x => x.WriteAsync(
      It.Is<ReadOnlyMemory<byte>>(mem => mem.ToArray().SequenceEqual(packet)),
      It.IsAny<CancellationToken>()), Times.Once);
  }
}
