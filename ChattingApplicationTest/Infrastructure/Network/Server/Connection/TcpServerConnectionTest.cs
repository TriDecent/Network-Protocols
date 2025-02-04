using System.Net;
using System.Net.Sockets;
using ChattingApplication.Infrastructure.Network.Server.Connection;
using NUnit.Framework;

namespace ChattingApplicationTest.Infrastructure.Network.Server.Connection;

[TestFixture]
public class TcpServerConnectionTest
{
  private TcpListener _listener;
  private TcpServerConnection _cut;
  private readonly IPEndPoint _endpoint = new(IPAddress.Loopback, 5000);

  [SetUp]
  public void Setup()
  {
    _listener = new TcpListener(_endpoint);
    _cut = new TcpServerConnection(_listener);
  }

  [TearDown]
  public void TearDown()
  {
    _cut.Dispose();
  }

  [Test]
  public void StartListening_ShouldSetListeningAndRunningFlagsToBeTrue_WhenCalled()
  {
    _cut.StartListening();

    Assert.Multiple(() =>
    {
      Assert.That(_cut.IsListening, Is.True);
      Assert.That(_cut.IsRunning, Is.True);
    });
  }

  [Test]
  public void StartListening_ShouldNotStartAgain_WhenAlreadyListening()
  {
    _cut.StartListening();
    var initialEndpoint = _cut.LocalEndPoint;

    _cut.StartListening();

    Assert.Multiple(() =>
    {
      Assert.That(_cut.IsListening, Is.True);
      Assert.That(_cut.LocalEndPoint, Is.EqualTo(initialEndpoint));
    });
  }

  [Test]
  public void StopListening_ShouldStopListeningAndUpdateFlag_WhenCalled()
  {
    _cut.StartListening();

    _cut.StopListening();

    Assert.Multiple(() =>
    {
      Assert.That(_cut.IsListening, Is.False);
      Assert.That(_cut.IsRunning, Is.True);
    });
  }

  [Test]
  public void StopListening_ShouldDoNothing_WhenNotListening()
  {
    _cut.StopListening();

    Assert.Multiple(() =>
    {
      Assert.That(_cut.IsListening, Is.False);
      Assert.That(_cut.IsRunning, Is.False);
    });
  }

  [Test]
  public void ShutDown_ShouldStopListeningAndRunning_WhenCalled()
  {
    _cut.StartListening();

    _cut.ShutDown();

    Assert.Multiple(() =>
    {
      Assert.That(_cut.IsListening, Is.False);
      Assert.That(_cut.IsRunning, Is.False);
    });
  }

  [Test]
  public void ShutDown_ShouldDoNothing_WhenNotRunning()
  {
    _cut.ShutDown();

    Assert.Multiple(() =>
    {
      Assert.That(_cut.IsListening, Is.False);
      Assert.That(_cut.IsRunning, Is.False);
    });
  }

  [Test]
  public async Task AcceptClientAsync_ShouldAcceptClient_WhenClientConnects()
  {
    _cut.StartListening();
    using var client = new TcpClient();
    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

    var connectionTask = _cut.AcceptClientAsync(cts.Token);
    await client.ConnectAsync("127.0.0.1", ((IPEndPoint)_listener.LocalEndpoint).Port);
    var acceptedClient = await connectionTask;

    Assert.That(acceptedClient.Connected, Is.True);
    Assert.That(client.Connected, Is.True);

    acceptedClient.Dispose();
  }

  [Test]
  public void AcceptClientAsync_ShouldThrowOperationCanceledException_WhenTimeout()
  {
    _cut.StartListening();
    using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

    Assert.ThrowsAsync<OperationCanceledException>(async () =>
    {
      await _cut.AcceptClientAsync(cts.Token);
    });
  }

  [Test]
  public void LocalEndPoint_ShouldReturnCorrectEndpoint_WhenCalled()
    => Assert.That(_cut.LocalEndPoint, Is.EqualTo(_endpoint));

  [Test]
  public void Dispose_ShouldDisposeListener_WhenCalled()
  {
    _cut.StartListening();

    _cut.Dispose();

    Assert.Multiple(() =>
    {
      Assert.That(_cut.IsListening, Is.False);
      Assert.That(_cut.IsRunning, Is.False);
    });
  }
}
