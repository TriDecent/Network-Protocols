using System.Net;
using ChattingApplication.Enums;
using ChattingApplication.Events;
using ChattingApplication.Models;

namespace ChattingApplication.Client;

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
