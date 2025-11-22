using System.Net;
using System.Net.Security;
using ChattingApplication.Core.Interfaces;
using ChattingApplication.Core.Sockets;

namespace ChattingApplication.Infrastructure.Network.Server.Connection;

public class SslServerConnectionDecorator(
  IServerConnection innerConnection,
  SslServerAuthenticationOptions options) : IServerConnection
{
  private readonly IServerConnection _innerConnection = innerConnection;
  private readonly SslServerAuthenticationOptions _options = options;

  public bool IsListening => _innerConnection.IsListening;
  public bool IsRunning => _innerConnection.IsRunning;
  public IPEndPoint LocalEndPoint => _innerConnection.LocalEndPoint;

  public async Task<ITcpClient> AcceptClientAsync(CancellationToken token)
  {
    var rawClient = await _innerConnection.AcceptClientAsync(token);

    var secureClient = new ServerSslTcpClient(rawClient, _options);

    await secureClient.PerformHandshakeAsync(token);

    return secureClient;
  }

  public void StartListening() => _innerConnection.StartListening();
  public void StopListening() => _innerConnection.StopListening();
  public void ShutDown() => _innerConnection.ShutDown();

  public void Dispose()
  {
    _innerConnection.Dispose();
  }
}