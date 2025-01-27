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
  Task SendUnicastMessageAsync(ClientSessionInfo clientInfo, Models.Message message);
  IPEndPoint ServerEndPoint { get; }
  ServerState State { get; }
  IReadOnlyList<ClientSessionInfo> ClientsInfo { get; }
  int ConnectedClientsCount { get; }
  event EventHandler<int>? ClientsCountChangedEventHandler;
  event EventHandler<ClientSessionInfoEventArgs>? ClientConnectedEventHandler;
  event EventHandler<ClientSessionInfoEventArgs>? ClientDisconnectedEventHandler;
  event EventHandler<MessageReceivedEventArgs>? MessageReceivedEventHandler;
  event EventHandler<StateChangedEventArgs>? StateChangedEventHandler;
}