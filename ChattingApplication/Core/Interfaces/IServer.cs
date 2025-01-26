using ChattingApplication.Common.Enums;
using ChattingApplication.Common.Events;
using ChattingApplication.Core.Models;
using System.Net;

namespace ChattingApplication.Core.Interfaces;

public interface IServer : IDisposable
{
  void StartListeningForConnections();
  void StopListeningForConnections();
  void ShutdownAllConnections();
  Task HandleIncomingConnectionsAsync();
  Task BroadcastMessageToAllClientsAsync(Models.Message message);
  IPEndPoint ServerEndPoint { get; }
  ServerState State { get; }
  int ConnectedClients { get; }
  event EventHandler<int>? ClientsCountChangedEventHandler;
  event EventHandler<ClientSessionInfo>? ClientConnectedEventHandler;
  event EventHandler<ClientSessionInfo>? ClientDisconnectedEventHandler;
  event EventHandler<MessageReceivedEventArgs>? MessageReceivedEventHandler;
  event EventHandler<StateChangedEventArgs>? StateChangedEventHandler;
}