using ChattingApplication.Common.Enums;
using ChattingApplication.Core.Models;
using System.Net;

namespace ChattingApplication.Core.Interfaces;

public interface IServer : IDisposable
{
  IPEndPoint ServerEndPoint { get; }
  ServerState State { get; }
  IReadOnlyList<ClientSessionInfo> ClientsInfo { get; }
  
  void StartListeningForConnections();
  void StopListeningForConnections();
  void ShutdownAllConnections();
  Task HandleIncomingConnectionsAsync();
}