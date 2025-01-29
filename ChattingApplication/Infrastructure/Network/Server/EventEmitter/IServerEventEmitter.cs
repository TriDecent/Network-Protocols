using ChattingApplication.Common.Enums;
using ChattingApplication.Common.Events;
using ChattingApplication.Core.Models;
using Message = ChattingApplication.Core.Models.Message;

namespace ChattingApplication.Infrastructure.Network.Server.EventEmitter;

public interface IServerEventEmitter
{
  event EventHandler<StateChangedEventArgs>? StateChanged;
  event EventHandler<MessageReceivedEventArgs>? BroadcastMessageReceived;
  event EventHandler<MessageReceivedEventArgs>? UnicastMessageReceived;
  event EventHandler<int>? ClientsCountChanged;
  event EventHandler<ClientSessionInfoEventArgs>? ClientConnected;
  event EventHandler<ClientSessionInfoEventArgs>? ClientDisconnected;

  void EmitChangedState(ServerState changedState);
  void EmitReceivedBroadcastMessage(Message message);
  void EmitReceivedUnicastMessage(Message message);
  void EmitChangedClientsCount(int clientsCount);
  void EmitConnectedClient(ClientSessionInfo connectedClient);
  void EmitDisconnectedClient(ClientSessionInfo disconnectedClient);
}
