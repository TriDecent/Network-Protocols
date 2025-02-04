using ChattingApplication.Common.Enums;
using ChattingApplication.Core.Interfaces;
using ChattingApplication.Core.Models;
using ChattingApplication.Core.Serializers;
using ChattingApplication.Infrastructure.Network.Client.Connection;
using ChattingApplication.Infrastructure.Network.Client.EventEmitter;
using Microsoft.VisualBasic.Devices;
using Moq;
using NUnit.Framework;
using System.Net;
using System.Net.Sockets;
using static ChattingApplication.Core.Interfaces.IClient;

namespace ChattingApplicationTest.Infrastructure.Network;

[TestFixture]
public class ClientTest
{
  private Mock<IClientConnection> _connectionMock;
  private Mock<IMessageSerializer> _serializerMock;
  private Mock<IClientEventEmitter> _eventEmitterMock;
  private ClientInfo _clientInfo;
  private ChattingApplication.Infrastructure.Network.Client.Client _cut;

  [SetUp]
  public void SetUp()
  {
    _connectionMock = new Mock<IClientConnection>();
    _serializerMock = new Mock<IMessageSerializer>();
    _eventEmitterMock = new Mock<IClientEventEmitter>();
    _clientInfo = new ClientInfo("test-id", "Test Client");
    _cut = new ChattingApplication.Infrastructure.Network.Client.Client(
      _connectionMock.Object,
      _clientInfo,
      _serializerMock.Object,
      _eventEmitterMock.Object);
  }

  [Test]
  public async Task ConnectServerAsync_ShouldEmitConnectedState_WhenConnectionSucceeds()
  {
    var endpoint = new IPEndPoint(IPAddress.Loopback, 5000);
    _connectionMock
      .Setup(mock => mock.ConnectAsync(endpoint, It.IsAny<CancellationToken>()))
      .ReturnsAsync(new ConnectionResult { Success = true });

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
  public async Task ConnectServerAsync_ShouldPerformInitialHandshake_WhenConnectionSucceeds()
  {
    var endpoint = new IPEndPoint(IPAddress.Loopback, 5000);
    var sentMessages = new List<ReadOnlyMemory<byte>>();

    _connectionMock
      .Setup(mock => mock.ConnectAsync(endpoint, It.IsAny<CancellationToken>()))
      .ReturnsAsync(new ConnectionResult { Success = true });

    _connectionMock
      .Setup(mock => mock.SendBytesAsync(
        It.IsAny<ReadOnlyMemory<byte>>(),
        It.IsAny<CancellationToken>()))
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
  public void DisconnectFromServer_EmitsDisconnectedState_WhenCalled()
  {
    _cut.DisconnectFromServer();

    _eventEmitterMock.Verify(x => x.EmitStateChanged(ClientState.Disconnecting), Times.Once);
    _eventEmitterMock.Verify(x => x.EmitStateChanged(ClientState.Disconnected), Times.Once);
    _connectionMock.Verify(x => x.Disconnect(), Times.Once);
  }

  [Test]
  public void UpdateName_ShouldUpdateClientInfoName_WhenCalled()
  {
    const string newName = "New Test Name";

    _cut.UpdateName(newName);

    Assert.That(_cut.ClientInfo.Name, Is.EqualTo(newName));
  }

  [Test]
  public async Task SendMessageAsync_ShouldSendPreparedMessage_WhenCalled()
  {
    var message = new Message(
      _clientInfo,
      Array.Empty<byte>(),
      MessageType.Text,
      Target.All,
      MessageRequest.None);

    var preparedBytes = new byte[] { 1, 2, 3 };
    _serializerMock
      .Setup(x => x.SerializeMessageToBytesAsync(message))
      .ReturnsAsync(preparedBytes);


    await _cut.SendMessageAsync(message);


    _connectionMock.Verify(
      mock => mock.SendBytesAsync(
        It.Is<ReadOnlyMemory<byte>>(memory =>
          memory.ToArray().SequenceEqual(preparedBytes)),
        It.IsAny<CancellationToken>()),
      Times.Once);
  }

  [Test]
  public void Dispose_ShouldDisposeConnection_WhenCalled()
  {
    _cut.Dispose();

    _connectionMock.Verify(x => x.Dispose(), Times.Once);
  }
}
