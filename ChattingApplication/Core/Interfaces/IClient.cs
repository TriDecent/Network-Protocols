using ChattingApplication.Common.Enums;
using ChattingApplication.Common.Events;
using ChattingApplication.Core.Models;
using System.Net;

namespace ChattingApplication.Core.Interfaces;

public interface IClient : IDisposable
{
  Task<ConnectionResult> ConnectServerAsync(IPEndPoint ipEndPoint);
  void DisconnectFromServer();
  Task SendMessageAsync(Models.Message message);
  void UpdateName(string newName);
  ClientInfo ClientInfo { get; }
  ClientState State { get; }

  public class ConnectionResult
  {
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
  }
}
