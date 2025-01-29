using ChattingApplication.Common.Enums;
using ChattingApplication.Common.Events;

namespace ChattingApplication.Infrastructure.Network.Client;

public interface IClientEventEmitter
{
  event EventHandler<StateChangedEventArgs>? StateChanged;
  event EventHandler<MessageReceivedEventArgs>? BroadcastMessageReceived;
  event EventHandler<MessageReceivedEventArgs>? UnicastMessageReceived;

  void EmitStateChanged(ClientState state);
  void EmitBroadcastMessageReceived(Core.Models.Message message);
  void EmitUnicastMessageReceived(Core.Models.Message message);
}
