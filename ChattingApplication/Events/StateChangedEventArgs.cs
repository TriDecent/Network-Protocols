namespace ChattingApplication;

internal class StateChangedEventArgs(
    ServerState? serverState, ClientState? clientState) : EventArgs
{
  public ServerState? ServerState { get; } = serverState;
  public ClientState? ClientState { get; } = clientState;
}
