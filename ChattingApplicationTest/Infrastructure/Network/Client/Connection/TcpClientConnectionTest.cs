using ChattingApplication.Core.Interfaces;
using ChattingApplication.Core.Models;
using ChattingApplication.Infrastructure.Network.Client.Connection;
using Moq;
using NUnit.Framework;
using System.Net;
using System.Net.Sockets;

namespace ChattingApplicationTest.Infrastructure.Network.Client.Connection;

[TestFixture]
public class TcpClientConnectionTest
{
  private Mock<ITcpClient> _tcpClientMock;
  private TcpClientConnection _cut;
  private IPEndPoint _endPoint;

  [SetUp]
  public void Setup()
  {
    _tcpClientMock = new Mock<ITcpClient>();
    _cut = new TcpClientConnection(_tcpClientMock.Object);
    _endPoint = new IPEndPoint(IPAddress.Loopback, 5000);
  }

  [Test]
  public void IsConnected_ShouldReturnClientConnectedStatus()
  {
    // Arrange
    _tcpClientMock.Setup(x => x.Connected).Returns(true);

    // Act & Assert
    Assert.That(_cut.IsConnected, Is.True);
  }

  [Test]
  public async Task ConnectAsync_WhenSuccessful_ReturnsSuccessResult()
  {
    // Arrange
    _tcpClientMock
      .Setup(mock => mock.ConnectAsync(_endPoint, It.IsAny<CancellationToken>()))
      .Returns(Task.CompletedTask);

    // Act
    var result = await _cut.ConnectAsync(_endPoint, CancellationToken.None);

    // Assert
    Assert.Multiple(() =>
    {
      Assert.That(result.Success, Is.True);
      Assert.That(result.ErrorMessage, Is.Null);
    });
  }

  [Test]
  public async Task ConnectAsync_WhenSocketException_ReturnsFailureResult()
  {
    // Arrange
    _tcpClientMock
      .Setup(mock => mock.ConnectAsync(_endPoint, It.IsAny<CancellationToken>()))
      .ThrowsAsync(new SocketException((int)SocketError.ConnectionRefused));

    // Act
    var result = await _cut.ConnectAsync(_endPoint, CancellationToken.None);

    // Assert
    Assert.Multiple(() =>
    {
      Assert.That(result.Success, Is.False);
      Assert.That(result.ErrorMessage, Is.EqualTo("Could not connect to the server. Please try again later."));
    });
  }

  [Test]
  public void Disconnect_WhenConnected_ShouldCloseAndReplaceClient()
  {
    // Arrange
    _tcpClientMock.Setup(mock => mock.Connected).Returns(true);

    // Act
    _cut.Disconnect();

    // Assert
    _tcpClientMock.Verify(mock => mock.Close(), Times.Once);
  }

  [Test]
  public async Task SendBytesAsync_WhenConnected_ShouldWriteToStream()
  {
    // Arrange
    var mockStream = new Mock<Stream>();
    var testData = new byte[] { 1, 2, 3 };

    _tcpClientMock.Setup(mock => mock.GetStream()).Returns(mockStream.Object);

    // Act
    await _cut.SendBytesAsync(testData, CancellationToken.None);

    // Assert
    mockStream.Verify(mock => mock.WriteAsync(
      It.Is<ReadOnlyMemory<byte>>(
        @byte => @byte.ToArray().SequenceEqual(testData)),
        It.IsAny<CancellationToken>()),
      Times.Once);
  }

  [Test]
  public void SendBytesAsync_WhenIOException_ShouldDisconnectAndRethrow()
  {
    // Arrange
    var mockStream = new Mock<Stream>();
    mockStream
      .Setup(mock => mock.WriteAsync(
        It.IsAny<ReadOnlyMemory<byte>>(),
        It.IsAny<CancellationToken>()))
      .ThrowsAsync(new IOException());

    _tcpClientMock.Setup(mock => mock.GetStream()).Returns(mockStream.Object);
    _tcpClientMock.Setup(mock => mock.Connected).Returns(true);

    // Act & Assert
    var ex = Assert.ThrowsAsync<IOException>(async () =>
      await _cut.SendBytesAsync(new byte[] { 1, 2, 3 }, CancellationToken.None));

    _tcpClientMock.Verify(mock => mock.Close(), Times.Once);
  }

  [Test]
  public void Dispose_ShouldDisposeClient()
  {
    // Act
    _cut.Dispose();

    // Assert
    _tcpClientMock.Verify(mock => mock.Dispose(), Times.Once);
  }
}
