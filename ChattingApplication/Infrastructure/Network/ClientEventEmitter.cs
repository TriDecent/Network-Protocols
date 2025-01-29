using ChattingApplication.Common.Enums;
using ChattingApplication.Common.Events;

namespace ChattingApplication.Infrastructure.Network;

public class ClientEventEmitter : IClientEventEmitter
{
  public event EventHandler<StateChangedEventArgs>? StateChanged;
  public event EventHandler<MessageReceivedEventArgs>? BroadcastMessageReceived;
  public event EventHandler<MessageReceivedEventArgs>? UnicastMessageReceived;

  public void EmitStateChanged(ClientState state)
  {
    StateChanged?.Invoke(this, new StateChangedEventArgs(null, state));
  }

  public void EmitBroadcastMessageReceived(Core.Models.Message message)
    => BroadcastMessageReceived?.Invoke(this,
      new MessageReceivedEventArgs(message, message.Type));

  public void EmitUnicastMessageReceived(Core.Models.Message message)
    => UnicastMessageReceived?.Invoke(this,
      new MessageReceivedEventArgs(message, message.Type));
}
