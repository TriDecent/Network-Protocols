using ChattingApplication.Core.Interfaces;
using ChattingApplication.Infrastructure.Network.Server.Connection;
using Moq;
using NUnit.Framework;
using System.Net;
using System.Net.Sockets;

namespace ChattingApplicationTest.Infrastructure.Network.Server.Connection;

[TestFixture]
public class TcpServerConnectionTest
{
  private Mock<ITcpListener> _listenerMock;
  private TcpServerConnection _cut;
  private readonly IPEndPoint _endpoint = new(IPAddress.Loopback, 5000);

  [SetUp]
  public void Setup()
  {
    _listenerMock = new Mock<ITcpListener>();
    _listenerMock.Setup(mock => mock.LocalEndpoint).Returns(_endpoint);
    _cut = new TcpServerConnection(_listenerMock.Object);
  }

  [Test]
  public void StartListening_ShouldSetFlagsAndStartListener_WhenCalled()
  {
    // Act
    _cut.StartListening();

    // Assert
    Assert.Multiple(() =>
    {
      Assert.That(_cut.IsListening, Is.True);
      Assert.That(_cut.IsRunning, Is.True);
    });
    _listenerMock.Verify(x => x.Start(), Times.Once);
  }

  [Test]
  public void StartListening_ShouldNotStartAgain_WhenAlreadyListening()
  {
    // Arrange
    _cut.StartListening();

    // Act
    _cut.StartListening();

    // Assert
    _listenerMock.Verify(mock => mock.Start(), Times.Once);
  }

  [Test]
  public void StopListening_ShouldUpdateFlagAndStopListener_WhenListening()
  {
    // Arrange
    _cut.StartListening();

    // Act
    _cut.StopListening();

    // Assert
    Assert.Multiple(() =>
    {
      Assert.That(_cut.IsListening, Is.False);
      Assert.That(_cut.IsRunning, Is.True);
    });
    _listenerMock.Verify(x => x.Stop(), Times.Once);
  }

  [Test]
  public void ShutDown_ShouldStopListenerAndUpdateFlags_WhenRunning()
  {
    // Arrange 
    _cut.StartListening();

    // Act
    _cut.ShutDown();

    // Assert
    Assert.Multiple(() =>
    {
      Assert.That(_cut.IsListening, Is.False);
      Assert.That(_cut.IsRunning, Is.False);
    });
    _listenerMock.Verify(x => x.Stop(), Times.Once);
  }

  [Test]
  public async Task AcceptClientAsync_ShouldDelegateToListener()
  {
    // Arrange
    var expectedClient = new TcpClient();
    _listenerMock
      .Setup(mock => mock.AcceptTcpClientAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(expectedClient);

    // Act
    var result = await _cut.AcceptClientAsync(CancellationToken.None);

    // Assert
    Assert.That(result, Is.EqualTo(expectedClient));
    _listenerMock.Verify(
      mock => mock.AcceptTcpClientAsync(It.IsAny<CancellationToken>()),
      Times.Once);
  }

  [Test]
  public void Dispose_ShouldCleanupAndUpdateFlags()
  {
    // Act
    _cut.Dispose();

    // Assert
    Assert.Multiple(() =>
    {
      Assert.That(_cut.IsListening, Is.False);
      Assert.That(_cut.IsRunning, Is.False);
    });
    _listenerMock.Verify(mock => mock.Dispose(), Times.Once);
  }
}
