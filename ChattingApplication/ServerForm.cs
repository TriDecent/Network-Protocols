using System.Net;
using System.Net.Sockets;
using System.Text;
using ChattingApplication.Enums;
using ChattingApplication.Events;
using ChattingApplication.Server;
using ChattingApplication.Utils;

namespace ChattingApplication;

public partial class ServerForm : Form
{
  private readonly Button _startServerButton, _stopServerButton, _shutdownButton;
  private readonly Button _attachButton, _detachButton;
  private readonly Label _stateLabel;
  private readonly Label _connectedClientsLabel;
  private readonly RichTextBox _chatDisplayArea;
  private readonly TextBox _messageTextbox;

  private readonly ChatMessageRenderer _chatRenderer;

  private readonly Server.Server _server; // only for better performance

  private bool _isSendingImage;

  public ServerForm(Server.Server server)
  {
    InitializeComponent();

    _startServerButton = btnStart;
    _stopServerButton = btnStop;
    _shutdownButton = btnShutDown;
    _stateLabel = lblState;
    _connectedClientsLabel = lblConnectedClients;
    _attachButton = btnAttach;
    _detachButton = btnDetach;
    _chatDisplayArea = rtbDialogArea;
    _messageTextbox = txtMessage;

    _chatRenderer = new ChatMessageRenderer(_chatDisplayArea);

    _server = server;

    _server.MessageReceivedEventHandler += OnMessageReceived;
    _server.StateChangedEventHandler += OnStateChanged;
    _server.ClientsChangedEventHandler += (s, connectedClientsCount)
      => _connectedClientsLabel.Text = connectedClientsCount.ToString();

    FormClosing += (s, e) => _server?.Dispose();
  }

  private void OnStateChanged(object? sender, StateChangedEventArgs e)
  {
    var serverState = e.ServerState;
    _stateLabel.Text = serverState == ServerState.Listening
      ? $"State: {serverState} at {_server.ServerEndPoint.Address}:{_server.ServerEndPoint.Port}"
      : $"State: {serverState}";

    var isServerActive = serverState == ServerState.Listening;
    _startServerButton.Enabled = !isServerActive;
    _stopServerButton.Enabled = isServerActive;
    _startServerButton.Visible = !isServerActive;
    _stopServerButton.Visible = isServerActive;
    _shutdownButton.Visible = isServerActive;
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

  private void BtnShutDown_Click(object sender, EventArgs e)
    => _server.ShutdownAllConnections();

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
    await _server.BroadcastImageToAllClientsAsync(image);
    _chatRenderer.DisplayImage("Server", image, true);
  }

  private async Task SendMessageAsync(string message)
  {
    await _server.BroadcastTextToAllClientsAsync(message);
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
