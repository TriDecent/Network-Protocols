using ChattingApplication.Common.Enums;
using ChattingApplication.Core.Models;
using ChattingApplication.Core.Serializers;
using ChattingApplication.Infrastructure.Network.Server.Connection;
using ChattingApplication.Infrastructure.Network.Server.EventEmitter;
using Moq;
using NUnit.Framework;
using System.Net;

namespace ChattingApplicationTest.Infrastructure.Network;

[TestFixture]
public class ServerTest
{
  private Mock<IServerConnection> _connectionMock;
  private Mock<IMessageSerializer> _serializerMock;
  private Mock<IServerEventEmitter> _eventEmitterMock;
  private ChattingApplication.Infrastructure.Network.Server.Server _cut;


  [SetUp]
  public void Setup()
  {
    _connectionMock = new Mock<IServerConnection>();
    _serializerMock = new Mock<IMessageSerializer>();
    _eventEmitterMock = new Mock<IServerEventEmitter>();

    _cut = new ChattingApplication.Infrastructure.Network.Server.Server(
      _connectionMock.Object,
      _serializerMock.Object,
      _eventEmitterMock.Object);
  }

  [Test]
  public void StartListeningForConnections_ShouldUpdateState_WhenStarting()
  {
    // Arrange
    _connectionMock.Setup(x => x.IsListening).Returns(false);

    // Act
    _cut.StartListeningForConnections();

    // Assert
    _connectionMock.Verify(x => x.StartListening(), Times.Once);
    _eventEmitterMock.Verify(x => x.EmitChangedState(ServerState.Starting), Times.Once);
    _eventEmitterMock.Verify(x => x.EmitChangedState(ServerState.Listening), Times.Once);
  }

  [Test]
  public void StopListeningForConnections_ShouldUpdateState_WhenStopping()
  {
    // Arrange
    _connectionMock.Setup(x => x.IsListening).Returns(true);

    // Act
    _cut.StopListeningForConnections();

    // Assert
    _connectionMock.Verify(x => x.StopListening(), Times.Once);
    _eventEmitterMock.Verify(x => x.EmitChangedState(ServerState.Stopping), Times.Once);
    _eventEmitterMock.Verify(x => x.EmitChangedState(ServerState.Stopped), Times.Once);
  }

  [Test]
  public void ShutdownAllConnections_ShouldUpdateState_WhenShuttingDown()
  {
    // Arrange
    _connectionMock.Setup(x => x.IsRunning).Returns(true);

    // Act
    _cut.ShutdownAllConnections();

    // Assert
    _connectionMock.Verify(x => x.ShutDown(), Times.Once);
    _eventEmitterMock.Verify(x => x.EmitChangedState(ServerState.ShuttingDown), Times.Once);
    _eventEmitterMock.Verify(x => x.EmitChangedState(ServerState.Shutdown), Times.Once);
  }

  [Test]
  public async Task HandleIncomingConnections_ShouldAcceptClients()
  {
    // refactor ClientSessionInfo to use a wrapper of TcpClient to implement the test 
  }

  [Test]
  public async Task BroadcastMessageToAllClients_ShouldSerializeAndBroadcastMessage()
  {
    var message = new Message(
      new ClientInfo("1", "Test"),
      Array.Empty<byte>(),
      MessageType.Text,
      Target.All,
      MessageRequest.None);

    var serializedBytes = new byte[] { 1, 2, 3 };
    _serializerMock
      .Setup(x => x.SerializeMessageToBytesAsync(message))
      .ReturnsAsync(serializedBytes);

    await _cut.BroadcastMessageToAllClientsAsync(message);

    _serializerMock.Verify(
      x => x.SerializeMessageToBytesAsync(message),
      Times.Once);
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
    _connectionMock.Setup(x => x.LocalEndPoint).Returns(expectedEndpoint);

    Assert.That(_cut.ServerEndPoint, Is.EqualTo(expectedEndpoint));
  }

  [Test]
  public async Task SendClientsInfoToClient_ShouldSendCurrentClientsList()
  {
    // refactor ClientSessionInfo to use a wrapper of TcpClient to implement the test 
  }

  [Test]
  public async Task ForwardMessageToClient_ShouldSendMessageToSpecificClient()
  {
    // refactor ClientSessionInfo to use a wrapper of TcpClient to implement the test 
  }
}
