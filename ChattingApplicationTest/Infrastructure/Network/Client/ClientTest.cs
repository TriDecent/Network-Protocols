using System.Net;
using ChattingApplication.Common.Enums;
using ChattingApplication.Core.Models;
using ChattingApplication.Infrastructure.Network.Client.Connection;
using ChattingApplication.Infrastructure.Network.Client.EventEmitter;
using ChattingApplication.Infrastructure.Network.Client.MessageProcessor;
using Moq;
using NUnit.Framework;
using static ChattingApplication.Core.Interfaces.IClient;

namespace ChattingApplicationTest.Infrastructure.Network.Client;

[TestFixture]
public class ClientTest
{
  private Mock<IClientConnection> _connectionMock;
  private Mock<IClientSideMessageProcessor> _messageProcessorMock;
  private Mock<IClientEventEmitter> _eventEmitterMock;
  private ClientInfo _clientInfo;
  private ChattingApplication.Infrastructure.Network.Client.Client _cut;

  [SetUp]
  public void SetUp()
  {
    _connectionMock = new Mock<IClientConnection>();
    _messageProcessorMock = new Mock<IClientSideMessageProcessor>();
    _eventEmitterMock = new Mock<IClientEventEmitter>();
    _clientInfo = new ClientInfo("test-id", "Test Client");
    _cut = new ChattingApplication.Infrastructure.Network.Client.Client(
      _connectionMock.Object,
      _clientInfo,
      _eventEmitterMock.Object,
      _messageProcessorMock.Object);
  }

  [Test]
  public async Task ConnectServerAsync_ShouldEmitConnectedState_WhenConnectionSucceeds()
  {
    var endpoint = new IPEndPoint(IPAddress.Loopback, 5000);
    _connectionMock
      .Setup(mock => mock.ConnectAsync(endpoint, It.IsAny<CancellationToken>()))
      .ReturnsAsync(new ConnectionResult { Success = true });

    _messageProcessorMock
      .Setup(m => m.PrepareOutgoingMessageAsync(It.IsAny<Message>()))
      .ReturnsAsync(new byte[] { 1, 2, 3 });

    _connectionMock
      .Setup(m => m.SendBytesAsync(It.IsAny<ReadOnlyMemory<byte>>(), It.IsAny<CancellationToken>()))
      .Returns(Task.CompletedTask);

    await _cut.ConnectServerAsync(endpoint);

    _eventEmitterMock.Verify(x => x.EmitStateChanged(ClientState.Connecting), Times.Once);
    _eventEmitterMock.Verify(x => x.EmitStateChanged(ClientState.Connected), Times.Once);
  }

  [Test]
  public async Task ConnectServerAsync_ShouldEmitFailedState_WhenConnectionFails()
  {
    var endpoint = new IPEndPoint(IPAddress.Loopback, 5000);
    _connectionMock
      .Setup(mock => mock.ConnectAsync(endpoint, It.IsAny<CancellationToken>()))
      .ReturnsAsync(new ConnectionResult { Success = false });

    await _cut.ConnectServerAsync(endpoint);

    _eventEmitterMock.Verify(x => x.EmitStateChanged(ClientState.Failed), Times.Once);
  }

  [Test]
  public async Task ConnectServerAsync_ShouldSendHandshakeMessages_WhenConnectionSucceeds()
  {
    var endpoint = new IPEndPoint(IPAddress.Loopback, 5000);
    var sentMessages = new List<ReadOnlyMemory<byte>>();

    _connectionMock
      .Setup(mock => mock.ConnectAsync(endpoint, It.IsAny<CancellationToken>()))
      .ReturnsAsync(new ConnectionResult { Success = true });

    _messageProcessorMock
      .Setup(m => m.PrepareOutgoingMessageAsync(It.IsAny<Message>()))
      .ReturnsAsync(new byte[] { 1, 2, 3 });

    _connectionMock
      .Setup(mock => mock.SendBytesAsync(It.IsAny<ReadOnlyMemory<byte>>(), It.IsAny<CancellationToken>()))
      .Callback<ReadOnlyMemory<byte>, CancellationToken>((bytes, _) => sentMessages.Add(bytes))
      .Returns(Task.CompletedTask);

    await _cut.ConnectServerAsync(endpoint);

    Assert.That(sentMessages.Count, Is.EqualTo(2));
    _connectionMock.Verify(
      mock => mock.SendBytesAsync(
        It.IsAny<ReadOnlyMemory<byte>>(),
        It.IsAny<CancellationToken>()
      ),
      Times.Exactly(2)
    );
  }

  [Test]
  public void DisconnectFromServer_EmitsDisconnectedState_AndDisconnectsConnection()
  {
    _cut.DisconnectFromServer();

    _eventEmitterMock.Verify(x => x.EmitStateChanged(ClientState.Disconnecting), Times.Once);
    _eventEmitterMock.Verify(x => x.EmitStateChanged(ClientState.Disconnected), Times.Once);
    _connectionMock.Verify(x => x.Disconnect(), Times.Once);
  }

  [Test]
  public void UpdateName_ShouldUpdateClientInfoName()
  {
    const string newName = "New Test Name";
    _cut.UpdateName(newName);
    Assert.That(_cut.ClientInfo.Name, Is.EqualTo(newName));
  }

  [Test]
  public async Task SendMessageAsync_ShouldSendPreparedMessage()
  {
    var message = new Message(
      _clientInfo,
      Array.Empty<byte>(),
      MessageType.Text,
      Target.All,
      MessageRequest.None);

    var preparedBytes = new byte[] { 1, 2, 3 };
    _messageProcessorMock
      .Setup(x => x.PrepareOutgoingMessageAsync(message))
      .ReturnsAsync(preparedBytes);

    _connectionMock
      .Setup(x => x.SendBytesAsync(It.IsAny<ReadOnlyMemory<byte>>(), It.IsAny<CancellationToken>()))
      .Returns(Task.CompletedTask);

    await _cut.SendMessageAsync(message);

    _connectionMock.Verify(
      mock => mock.SendBytesAsync(
        It.Is<ReadOnlyMemory<byte>>(memory =>
          memory.ToArray().SequenceEqual(preparedBytes)),
        It.IsAny<CancellationToken>()),
      Times.Once);
  }

  [Test]
  public void Dispose_ShouldDisposeConnection()
  {
    _cut.Dispose();
    _connectionMock.Verify(x => x.Dispose(), Times.Once);
  }
}
