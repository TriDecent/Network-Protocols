using ChattingApplication.Common.Enums;
using ChattingApplication.Core.Interfaces;
using ChattingApplication.Core.Models;
using ChattingApplication.Core.Serializers;
using ChattingApplication.Infrastructure.Network.Client.EventEmitter;
using ChattingApplication.Infrastructure.Network.Client.MessageProcessor;
using System.Net;
using System.Net.Sockets;
using static ChattingApplication.Core.Interfaces.IClient;
using Message = ChattingApplication.Core.Models.Message;

namespace ChattingApplication.Infrastructure.Network.Client;

public class Client : IClient
{
  private TcpClient _client;
  private readonly IClientEventEmitter _eventEmitter;
  private readonly IClientSideMessageProcessor _messageProcessor;
  public ClientInfo ClientInfo { get; private set; }
  private CancellationTokenSource _cts = new();
  public ClientState State { get; private set; } = ClientState.Disconnected;

  public Client(
    TcpClient tcpClient,
    ClientInfo clientInfo,
    IMessageSerializer serializer,
    IClientEventEmitter eventEmitter)
  {
    ClientInfo = clientInfo;
    _client = tcpClient;
    _eventEmitter = eventEmitter;
    _messageProcessor = new ClientSideMessageProcessor(
      serializer,
      eventEmitter,
      id => ClientInfo = ClientInfo with { Id = id });
  }

  public async Task<ConnectionResult> ConnectServerAsync(IPEndPoint ipEndPoint)
  {
    try
    {
      UpdateState(ClientState.Connecting);
      await _client.ConnectAsync(ipEndPoint);
      await TransferCurrentClientInfoToServer();
      await RequestClientIdFromServer();
      UpdateState(ClientState.Connected);

      _ = HandleReceivedMessageAsync().ContinueWith(cancelledTask =>
      {
        // Do nothing, valid cancellation
      }, TaskContinuationOptions.OnlyOnCanceled);

      return new ConnectionResult { Success = true };
    }
    catch (SocketException ex)
    {
      UpdateState(ClientState.Failed);
      return new ConnectionResult
      {
        Success = false,
        ErrorMessage = GetSocketErrorMessage(ex.SocketErrorCode)
      };
    }
  }

  public void DisconnectFromServer()
  {
    if (!_client.Connected) return;

    UpdateState(ClientState.Disconnecting);

    _cts.Cancel();
    _cts = new();

    _client.Close();
    _client = new TcpClient();

    UpdateState(ClientState.Disconnected);
  }

  public void UpdateName(string newName)
    => ClientInfo = ClientInfo with { Name = newName };

  public async Task SendMessageAsync(Message message)
  {
    var messageBytes = await _messageProcessor.PrepareOutgoingMessageAsync(message);
    await SendBytesAsync(messageBytes);
  }

  private async Task SendBytesAsync(ReadOnlyMemory<byte> bytes)
  {
    var stream = _client.GetStream();
    try
    {
      await stream.WriteAsync(bytes, _cts.Token);
    }
    catch (IOException ex)
    {
      UpdateState(ClientState.Disconnected);
      _client = new TcpClient();

      throw new IOException("Connection to server was lost", ex);
    }
  }

  private void UpdateState(ClientState state)
  {
    State = state;
    _eventEmitter.EmitStateChanged(state);
  }

  private async Task HandleReceivedMessageAsync()
  {
    try
    {
      var stream = _client.GetStream();
      await _messageProcessor.HandleMessageFromStreamAsync(stream, _cts.Token);
    }
    catch (EndOfStreamException)
    {
      DisconnectFromServer();
    }
  }

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

  private static string GetSocketErrorMessage(SocketError errorCode) => errorCode switch
  {
    SocketError.ConnectionRefused =>
      "Could not connect to the server. Please try again later.",
    SocketError.TimedOut =>
      "Connection attempt timed out. The server is not responding.",
    SocketError.HostUnreachable or SocketError.NetworkUnreachable =>
      "The server is not reachable. Please check your internet connection.",
    _ => "An unexpected error occurred. Please try again later."
  };

  public void Dispose()
  {
    _cts.Cancel();
    _cts.Dispose();
    _client.Dispose();
  }
}
