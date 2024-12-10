using System.Net;
using ChattingApplication.Enums;
using ChattingApplication.Events;

namespace ChattingApplication.Server;

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
  EventHandler<int>? ClientsChangedEventHandler { get; set; }
  EventHandler<MessageReceivedEventArgs>? MessageReceivedEventHandler { get; set; }
  EventHandler<StateChangedEventArgs>? StateChangedEventHandler { get; set; }
}