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
  ClientInfo ClientDetails { get; }
  ClientState State { get; }
  EventHandler<StateChangedEventArgs>? StatusChangedEventHandler { get; set; }
  EventHandler<MessageReceivedEventArgs>? MessageReceivedEventHandler { get; set; }

  public class ConnectionResult
  {
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
  }
}
