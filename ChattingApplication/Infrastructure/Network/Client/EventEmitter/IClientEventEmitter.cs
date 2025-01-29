using ChattingApplication.Common.Enums;
using ChattingApplication.Common.Events;
using Message = ChattingApplication.Core.Models.Message;

namespace ChattingApplication.Infrastructure.Network.Client.EventEmitter;

public interface IClientEventEmitter
{
  event EventHandler<StateChangedEventArgs>? StateChanged;
  event EventHandler<MessageReceivedEventArgs>? BroadcastMessageReceived;
  event EventHandler<MessageReceivedEventArgs>? UnicastMessageReceived;

  void EmitStateChanged(ClientState state);
  void EmitBroadcastMessageReceived(Message message);
  void EmitUnicastMessageReceived(Message message);
}
