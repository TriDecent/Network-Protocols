using ChattingApplication.Common.Enums;
using ChattingApplication.Common.Events;
using Message = ChattingApplication.Core.Models.Message;

namespace ChattingApplication.Infrastructure.Network.Client.EventEmitter;

public class ClientEventEmitter : IClientEventEmitter
{
  public event EventHandler<StateChangedEventArgs>? StateChanged;
  public event EventHandler<MessageReceivedEventArgs>? BroadcastMessageReceived;
  public event EventHandler<MessageReceivedEventArgs>? UnicastMessageReceived;

  public void EmitStateChanged(ClientState state)
    => StateChanged?.Invoke(this, new StateChangedEventArgs(null, state));

  public void EmitBroadcastMessageReceived(Message message)
  => BroadcastMessageReceived?.Invoke(this,
    new MessageReceivedEventArgs(message));

  public void EmitUnicastMessageReceived(Message message)
    => UnicastMessageReceived?.Invoke(this,
      new MessageReceivedEventArgs(message));
}
