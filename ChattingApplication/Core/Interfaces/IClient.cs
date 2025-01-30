using ChattingApplication.Common.Enums;
using ChattingApplication.Core.Models;
using ChattingApplication.Infrastructure.Network.Client.Connection;

namespace ChattingApplication.Core.Interfaces;

public interface IClient
{
  ClientInfo ClientInfo { get; }
  ClientState State { get; }

  public class ConnectionResult
  {
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
  }
}
