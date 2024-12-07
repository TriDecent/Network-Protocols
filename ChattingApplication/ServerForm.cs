using System.Net;
using System.Net.Sockets;
using ChattingApplication.Enums;
using ChattingApplication.Events;
using ChattingApplication.Utils;

namespace ChattingApplication;

public partial class ServerForm : Form
{
  private readonly Button _startServerButton, _stopServerButton;
  private readonly Button _sendMessageButton;
  private readonly Button _attachButton, _detachButton;
  private readonly Label _stateLabel;
  private readonly RichTextBox _chatDisplayArea;

  private readonly ChatMessageRenderer _chatRenderer;

  private readonly Server _server;

  public ServerForm()
  {
    InitializeComponent();

    _startServerButton = btnStart;
    _stopServerButton = btnStop;
    _sendMessageButton = btnSend;
    _stateLabel = lblState;
    _attachButton = btnAttach;
    _detachButton = btnDetach;
    _chatDisplayArea = rtbDialogArea;

    _chatRenderer = new ChatMessageRenderer(_chatDisplayArea);

    var tcpListener = new TcpListener(IPAddress.Any, 1211);
    _server = new Server(tcpListener);

    _server.MessageReceivedEventHandler += OnMessageReceived;
    _server.StateChangedEventHandler += OnStateChanged;
  }

  private void OnStateChanged(object? sender, StateChangedEventArgs e)
  {
    var serverState = e.ServerState;

    _stateLabel.Text = $"State: {serverState}";

    _startServerButton.Enabled = serverState != ServerState.Starting
      && serverState != ServerState.ShuttingDown;

    _stopServerButton.Enabled = _startServerButton.Enabled;

    _startServerButton.Visible = serverState != ServerState.Listening;
    _stopServerButton.Visible = !_startServerButton.Visible;
  }

  private void OnMessageReceived(object? sender, MessageReceivedEventArgs e)
  {
    if (e.Type == MessageType.Image)
    {
      var bytes = e.Content as byte[] ?? [];
      _chatRenderer.DisplayImage("Client", bytes.BytesToImage(), false);
      return;
    }

    var message = e.Content as string ?? "";
    _chatRenderer.DisplayMessage("Client", message, false);
  }

  private async void BtnStart_Click(object sender, EventArgs e)
  {
    _server.Start();
    await _server.HandleMultipleConnections();
  }

  private void BtnClose_Click(object sender, EventArgs e)
    => _server.Stop();
}
