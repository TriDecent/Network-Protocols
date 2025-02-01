using ChattingApplication.Common.Enums;
using ChattingApplication.Core.Interfaces;
using ChattingApplication.Core.Models;
using ChattingApplication.Core.Serializers;
using ChattingApplication.Infrastructure.Network.Client.Connection;
using ChattingApplication.Infrastructure.Network.Client.EventEmitter;
using ChattingApplication.Infrastructure.Network.Client.MessageProcessor;
using ChattingApplication.Infrastructure.Network.Client.Operations;
using System.Net;
using static ChattingApplication.Core.Interfaces.IClient;
using Message = ChattingApplication.Core.Models.Message;

namespace ChattingApplication.Infrastructure.Network.Client;

public class Client : IClient, IClientOperations, IDisposable
{
  public ClientInfo ClientInfo { get; private set; }
  public ClientState State { get; private set; } = ClientState.Disconnected;

  private CancellationTokenSource _cts = new();

  private readonly IClientConnection _connection;
  private readonly IClientSideMessageProcessor _messageProcessor;
  private readonly IClientEventEmitter _eventEmitter;

  public Client(
  IClientConnection connection,
  ClientInfo clientInfo,
  IMessageSerializer serializer,
  IClientEventEmitter eventEmitter)
  {
    ClientInfo = clientInfo;
    _connection = connection;
    _eventEmitter = eventEmitter;
    _messageProcessor = new ClientSideMessageProcessor(
      serializer,
      eventEmitter,
      id => ClientInfo = ClientInfo with { Id = id });
  }

  public async Task<ConnectionResult> ConnectServerAsync(IPEndPoint ipEndPoint)
  {
    UpdateState(ClientState.Connecting);
    var result = await _connection.ConnectAsync(ipEndPoint, _cts.Token);

    if (!result.Success)
    {
      UpdateState(ClientState.Failed);
      return result;
    }

    UpdateState(ClientState.Connected);
    await TransferCurrentClientInfoToServer();
    await RequestClientIdFromServer();
    _ = HandleReceivedMessageAsync();

    return result;
  }

  public void DisconnectFromServer()
  {
    UpdateState(ClientState.Disconnecting);

    _cts.Cancel();
    _cts = new();
    _connection.Disconnect();

    UpdateState(ClientState.Disconnected);
  }

  public void UpdateName(string newName)
    => ClientInfo = ClientInfo with { Name = newName };

  public async Task SendMessageAsync(Message message)
  {
    var messageBytes = await _messageProcessor.PrepareOutgoingMessageAsync(message);
    await _connection.SendBytesAsync(messageBytes, _cts.Token);
  }

  private async Task HandleReceivedMessageAsync()
  {
    try
    {
      var stream = _connection.GetStream();
      await _messageProcessor.HandleMessageFromStreamAsync(stream, _cts.Token);
    }
    catch (EndOfStreamException)
    {
      DisconnectFromServer();
    }
  }

  private void UpdateState(ClientState newState)
  {
    State = newState;
    _eventEmitter.EmitStateChanged(newState);
  }

  public void Dispose() => _connection.Dispose();

  private async Task TransferCurrentClientInfoToServer()
  {
    var message = new Message(
      ClientInfo, [], MessageType.Any, Target.Server, MessageRequest.None);

    await SendMessageAsync(message);
  }

  private async Task RequestClientIdFromServer()
  {
    var message = new Message(
      ClientInfo, [], MessageType.Any, Target.Server, MessageRequest.GetCreationUserId);

    await SendMessageAsync(message);
  }
}
