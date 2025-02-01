using System.Net;
using System.Net.Sockets;

namespace ChattingApplication.Infrastructure.Network.Server.Connection;

public class TcpServerConnection(TcpListener listener) : IServerConnection
{
  public bool IsListening => _isListening;
  public bool IsRunning => _isRunning;
  public IPEndPoint LocalEndPoint => (IPEndPoint)_listener.LocalEndpoint;

  private bool _isListening;
  private bool _isRunning;
  private readonly TcpListener _listener = listener;

  public void StartListening()
  {
    if (_isListening) return;

    _isListening = true;
    _isRunning = true;

    _listener.Start();
  }

  public void StopListening()
  {
    if (!_isListening) return;

    _isListening = false;
    _listener.Stop();
  }

  public void ShutDown()
  {
    if (!_isRunning) return;

    _isListening = false;
    _isRunning = false;

    _listener.Stop();
  }

  public async Task<TcpClient> AcceptClientAsync(CancellationToken token)
    => await _listener.AcceptTcpClientAsync(token);

  public void Dispose() => _listener.Dispose();
}
