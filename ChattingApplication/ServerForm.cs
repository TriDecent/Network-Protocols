using System.Net;
using System.Net.Sockets;
using System.Text;
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
  private readonly TextBox _messageTextbox;

  private readonly ChatMessageRenderer _chatRenderer;

  private readonly Server _server;

  private bool _isSendingImage;

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
    _messageTextbox = txtMessage;

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
    _server.StartListeningForConnections();

    await _server.HandleIncomingConnectionsAsync();
  }

  private void BtnStop_Click(object sender, EventArgs e)
    => _server.StopListeningForConnections();

  private async void BtnSend_ClickAsync(object sender, EventArgs e)
  {
    var message = _messageTextbox.Text.Trim();
    if (string.IsNullOrEmpty(message)) return;

    var sendTask = _isSendingImage ? SendImageAsync(message) : SendMessageAsync(message);
    await sendTask;

    ClearServerMessageInput();
  }

  private async Task SendImageAsync(string filePath)
  {
    var image = Image.FromFile(filePath);
    await _server.BroadcastImageToAllClients(image);
    _chatRenderer.DisplayImage("Server", image, true);
  }

  private async Task SendMessageAsync(string message)
  {
    await _server.BroadcastMessageToAllClients(message);
    _chatRenderer.DisplayMessage("Server", message, true);
  }

  private void BtnAttach_Click(object sender, EventArgs e)
  {
    var openFileDialog = new OpenFileDialog
    {
      Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif"
    };

    if (openFileDialog.ShowDialog() == DialogResult.OK)
    {
      _messageTextbox.Text = openFileDialog.FileName;
      _isSendingImage = true;
      _messageTextbox.Enabled = false;
      ToggleAttachDetachButtons();
    }
  }

  private void BtnDetach_Click(object sender, EventArgs e)
  {
    _isSendingImage = false;
    ClearServerMessageInput();
    ToggleAttachDetachButtons();
    _messageTextbox.Enabled = true;
  }

  private void ToggleAttachDetachButtons()
  {
    _attachButton.Visible = !_isSendingImage;
    _detachButton.Visible = _isSendingImage;
  }

  private void ClearServerMessageInput() => _messageTextbox.Text = string.Empty;
}
