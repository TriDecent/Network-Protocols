using System.Net;
using System.Net.Security;
using ChattingApplication.Core.Interfaces;
using ChattingApplication.Core.Sockets;
using Moq;
using NUnit.Framework;

namespace ChattingApplicationTest.Core.Sockets;

[TestFixture]
public class SslTcpClientBaseTest
{
  private Mock<ITcpClient> _innerClientMock;

  [SetUp]
  public void SetUp()
  {
    _innerClientMock = new Mock<ITcpClient>();
  }

  private class DummySslTcpClient(ITcpClient inner) : SslTcpClientBase(inner)
  {
    public override Task PerformHandshakeAsync(CancellationToken token) => Task.CompletedTask;
    public void SetSslStream(SslStream stream) => _sslStream = stream;
  }

  // FakeSslStream to avoid Moq issues with SslStream
  private class FakeSslStream : SslStream
  {
    public FakeSslStream() : base(new MemoryStream(), false) { }
    public override bool IsAuthenticated => true;
    public bool Disposed { get; private set; }
    protected override void Dispose(bool disposing)
    {
      Disposed = true;
      base.Dispose(disposing);
    }
  }

  [Test]
  public void Connected_ShouldReturnInnerClientConnected()
  {
    _innerClientMock.Setup(x => x.Connected).Returns(true);
    var dummy = new DummySslTcpClient(_innerClientMock.Object);
    Assert.That(dummy.Connected, Is.True);
  }

  [Test]
  public void GetStream_ShouldThrowIfHandshakeNotCompleted()
  {
    var dummy = new DummySslTcpClient(_innerClientMock.Object);
    Assert.Throws<InvalidOperationException>(() => dummy.GetStream());
  }

  [Test]
  public void GetStream_ShouldReturnSslStream_WhenAuthenticated()
  {
    var dummy = new DummySslTcpClient(_innerClientMock.Object);
    var sslStream = new FakeSslStream();
    dummy.SetSslStream(sslStream);

    var result = dummy.GetStream();
    Assert.That(result, Is.EqualTo(sslStream));
  }

  [Test]
  public void Dispose_ShouldDisposeSslStreamAndInnerClient()
  {
    var dummy = new DummySslTcpClient(_innerClientMock.Object);
    var sslStream = new FakeSslStream();
    dummy.SetSslStream(sslStream);

    dummy.Dispose();

    Assert.That(sslStream.Disposed, Is.True);
    _innerClientMock.Verify(x => x.Dispose(), Times.Once);
  }

  [Test]
  public void Close_ShouldDisposeSslStream()
  {
    var dummy = new DummySslTcpClient(_innerClientMock.Object);
    var sslStream = new FakeSslStream();
    dummy.SetSslStream(sslStream);

    dummy.Close();

    Assert.That(sslStream.Disposed, Is.True);
  }

  [Test]
  public async Task ConnectAsync_ShouldCallInnerClientConnectAsync()
  {
    var dummy = new DummySslTcpClient(_innerClientMock.Object);
    var ep = new IPEndPoint(IPAddress.Loopback, 1234);
    var token = new CancellationTokenSource().Token;

    _innerClientMock.Setup(x => x.ConnectAsync(ep, token)).Returns(Task.CompletedTask).Verifiable();

    await dummy.ConnectAsync(ep, token);

    _innerClientMock.Verify(x => x.ConnectAsync(ep, token), Times.Once);
  }
}
