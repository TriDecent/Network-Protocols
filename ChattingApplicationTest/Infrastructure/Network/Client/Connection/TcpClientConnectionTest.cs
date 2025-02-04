using System.Net;
using System.Net.Sockets;
using ChattingApplication.Infrastructure.Network.Client.Connection;
using NUnit.Framework;

namespace ChattingApplicationTest.Infrastructure.Network.Client.Connection;

[TestFixture]
public class TcpClientConnectionTest
{
  private TcpClientConnection _cut;
  private TcpClient _tcpClient;
  private IPEndPoint _endPoint;

  [SetUp]
  public void SetUp()
  {
    _tcpClient = new TcpClient();
    _cut = new TcpClientConnection(_tcpClient);
    _endPoint = new IPEndPoint(IPAddress.Loopback, 12345);
  }

  [TearDown]
  public void Cleanup()
  {
    _cut.Dispose();
  }

  [Test]
  public void IsConnected_WhenNotConnected_ReturnsFalse()
    => Assert.That(_cut.IsConnected, Is.False);


  [Test]
  public async Task ConnectAsync_WhenServerNotAvailable_ReturnsFailureResult()
  {
    var result = await _cut.ConnectAsync(_endPoint, CancellationToken.None);

    Assert.Multiple(() =>
    {
      Assert.That(result.Success, Is.False);
      Assert.That(result.ErrorMessage, Is.Not.Empty);
    });
  }

  [Test]
  public Task ConnectAsync_WhenCancelled_ThrowsTaskCanceledException()
  {
    using var cts = new CancellationTokenSource();
    cts.Cancel();

    Assert.ThrowsAsync<TaskCanceledException>(
      async () => await _cut.ConnectAsync(_endPoint, cts.Token));

    return Task.CompletedTask;
  }

  [Test]
  public void Disconnect_WhenNotConnected_DoesNotThrowException()
    => Assert.DoesNotThrow(_cut.Disconnect);

  [Test]
  public void GetStream_WhenNotConnected_ThrowsInvalidOperationException()
    => Assert.Throws<InvalidOperationException>(() => _cut.GetStream());

  [Test]
  public Task SendBytesAsync_WhenNotConnected_ThrowsInvalidOperationException()
  {
    var data = new Memory<byte>([1, 2, 3]);

    Assert.ThrowsAsync<InvalidOperationException>(async () => await _cut.SendBytesAsync(data, CancellationToken.None));

    return Task.CompletedTask;
  }

  [Test]
  public async Task SendBytesAsync_WhenConnected_SendsData()
  {
    var data = new Memory<byte>([1, 2, 3]);
    using var server = new TcpListener(IPAddress.Loopback, 0);
    server.Start();

    try
    {
      await _cut.ConnectAsync((IPEndPoint)server.LocalEndpoint, CancellationToken.None);

      Assert.DoesNotThrowAsync(async () => await _cut.SendBytesAsync(data, CancellationToken.None));
    }
    finally
    {
      server.Stop();
    }
  }

  [Test]
  public async Task SendBytesAsync_ConnectionInterrupted_ThrowsIOException()
  {
    var data = new Memory<byte>([1, 2, 3]);
    using var server = new TcpListener(IPAddress.Loopback, 0);
    server.Start();

    try
    {
      await _cut.ConnectAsync((IPEndPoint)server.LocalEndpoint, CancellationToken.None);

      server.Stop();

      Assert.ThrowsAsync<IOException>(async () =>
        await _cut.SendBytesAsync(data, CancellationToken.None));
    }
    finally
    {
      server.Stop();
    }
  }


  [Test]
  public async Task SendBytesAsync_ConnectionCanceled_ThrowsTaskCanceledException()
  {
    var data = new Memory<byte>([1, 2, 3]);
    using var server = new TcpListener(IPAddress.Loopback, 0);
    using var cts = new CancellationTokenSource();
    server.Start();

    try
    {
      await _cut.ConnectAsync((IPEndPoint)server.LocalEndpoint, cts.Token);

      cts.Cancel();

      Assert.ThrowsAsync<TaskCanceledException>(async () =>
        await _cut.SendBytesAsync(data, cts.Token));
    }
    finally
    {
      server.Stop();
    }
  }
}
