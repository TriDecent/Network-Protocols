using System.Net;
using ChattingApplication.Enums;
using ChattingApplication.Events;

namespace ChattingApplication.Client;

public interface IClient : IDisposable
{
  Task<ConnectionResult> ConnectServerAsync(IPEndPoint ipEndPoint);
  void DisconnectFromServer();
  Task SendTextAsync(string message);
  Task SendImageAsync(Image image);
  ClientState State { get; }
  EventHandler<StateChangedEventArgs>? StatusChangedEventHandler { get; set; }
  EventHandler<MessageReceivedEventArgs>? MessageReceivedEventHandler { get; set; }


  public class ConnectionResult
  {
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
  }
}
