using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ChattingApplication;

internal class Client(TcpClient client)
{
  private TcpClient _client = client;
  public ClientState State { get; private set; } = ClientState.Disconnected;
  public EventHandler<StateEventArgs>? StatusEventHandler;

  public async Task ConnectServerAsync(IPEndPoint ipEndPoint)
  {
    try
    {
      UpdateState(ClientState.Connecting);
      await _client.ConnectAsync(ipEndPoint);
      UpdateState(ClientState.Connected);
    }
    catch (SocketException)
    {
      UpdateState(ClientState.Failed);
      throw;
    }
  }

  public void DisconnectFromServer()
  {
    if (!_client.Connected) return;

    UpdateState(ClientState.Disconnecting);
    _client.Close();

    _client = new TcpClient();
    UpdateState(ClientState.Disconnected);
  }

  public void SendMessage(string message)
  {
    var bytes = Encoding.UTF8.GetBytes(message);
    var stream = _client.GetStream();
    stream.Write(bytes);
  }

  private void UpdateState(ClientState state)
  {
    State = state;
    RaiseChangedState();
  }

  private void RaiseChangedState()
    => StatusEventHandler?.Invoke(this, new StateEventArgs(null, State));
}

internal class StateEventArgs(ServerState? serverState, ClientState? clientState) : EventArgs
{
  public ServerState? ServerState = serverState;
  public ClientState? ClientState = clientState;
}