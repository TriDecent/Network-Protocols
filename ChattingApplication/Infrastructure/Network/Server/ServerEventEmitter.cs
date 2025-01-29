using ChattingApplication.Common.Enums;
using ChattingApplication.Common.Events;
using ChattingApplication.Core.Models;

namespace ChattingApplication.Infrastructure.Network.Server;

public class ServerEventEmitter : IServerEventEmitter
{
  public event EventHandler<StateChangedEventArgs>? StateChanged;
  public event EventHandler<MessageReceivedEventArgs>? BroadcastMessageReceived;
  public event EventHandler<MessageReceivedEventArgs>? UnicastMessageReceived;
  public event EventHandler<int>? ClientsCountChanged;
  public event EventHandler<ClientSessionInfoEventArgs>? ClientConnected;
  public event EventHandler<ClientSessionInfoEventArgs>? ClientDisconnected;

  public void EmitChangedState(ServerState changedState)
    => StateChanged?.Invoke(this, new StateChangedEventArgs(changedState, null));

  public void EmitReceivedBroadcastMessage(Core.Models.Message message)
    => BroadcastMessageReceived?.Invoke(this, new MessageReceivedEventArgs(message));

  public void EmitReceivedUnicastMessage(Core.Models.Message message)
    => UnicastMessageReceived?.Invoke(this, new MessageReceivedEventArgs(message));

  public void EmitChangedClientsCount(int clientsCount)
    => ClientsCountChanged?.Invoke(this, clientsCount);

  public void EmitConnectedClient(ClientSessionInfo connectedClient)
    => ClientConnected?.Invoke(this, new ClientSessionInfoEventArgs(connectedClient));

  public void EmitDisconnectedClient(ClientSessionInfo disconnectedClient)
    => ClientDisconnected?.Invoke(this, new ClientSessionInfoEventArgs(disconnectedClient));
}
