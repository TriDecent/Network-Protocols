using ChattingApplication.Common.Enums;
using ChattingApplication.Core.Interfaces;
using ChattingApplication.Core.Models;
using ChattingApplication.Core.Serializers;
using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using static ChattingApplication.Core.Interfaces.IClient;

namespace ChattingApplication.Infrastructure.Network;

public class Client(
  TcpClient tcpClient,
  ClientInfo clientDetails,
  IMessageSerializer serializer,
  IClientEventEmitter eventEmitter) : IClient
{
  private const int MESSAGE_CONTENT_SIZE_PREFIX_LENGTH = sizeof(int);
  private TcpClient _client = tcpClient;
  private readonly IMessageSerializer _serializer = serializer;
  private readonly IClientEventEmitter _eventEmitter = eventEmitter;
  public ClientInfo ClientInfo { get; private set; } = clientDetails;
  private CancellationTokenSource _cts = new();
  public ClientState State { get; private set; } = ClientState.Disconnected;

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

  public async Task SendMessageAsync(Core.Models.Message message)
  {
    var messageBytes = await _serializer.SerializeMessageToBytesAsync(message);

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
    var stream = _client.GetStream();

    var buffer = new byte[MESSAGE_CONTENT_SIZE_PREFIX_LENGTH];
    while (!_cts.Token.IsCancellationRequested)
    {
      try
      {
        await stream.ReadExactlyAsync(buffer.AsMemory(), _cts.Token);

        var contentLength = BinaryPrimitives.ReadInt32BigEndian(buffer);
        var contentBytes = new byte[contentLength];

        await stream.ReadExactlyAsync(contentBytes.AsMemory(), _cts.Token);

        var message = JsonSerializer.Deserialize<Core.Models.Message>(contentBytes)!;

        if (message.Target is Target.All)
        {
          _eventEmitter.EmitBroadcastMessageReceived(message);
          continue;
        }

        if (message.Target is Target.Individual &&
          message.Type is MessageType.CreationClientId)
        {
          ClientInfo = ClientInfo with
          {
            Id = JsonSerializer.Deserialize<string>(message.Content)!
          };
        }

        _eventEmitter.EmitUnicastMessageReceived(message);
      }
      catch (EndOfStreamException)
      {
        DisconnectFromServer();
        break;
      }
    }
  }

  private async Task TransferCurrentClientInfoToServer()
  {
    var message = new Core.Models.Message(
      ClientInfo, [], MessageType.Any, Target.Server, MessageRequest.None);

    await SendMessageAsync(message);
  }

  private async Task RequestClientIdFromServer()
  {
    var message = new Core.Models.Message(
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
