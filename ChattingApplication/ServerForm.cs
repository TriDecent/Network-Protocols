using System.Text;
using ChattingApplication.Common.Enums;
using ChattingApplication.Common.Events;
using ChattingApplication.Common.Utils;
using ChattingApplication.Core.Models;
using ChattingApplication.Infrastructure.Network;

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

  private readonly Server _server; // only for better performance

  private bool _isSendingImage;

  public ServerForm(Server server)
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
    _server.ClientsCountChangedEventHandler += (s, connectedClientsCount)
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
    var messageOwner = e.Message.Client.Name;

    if (e.Message.Type == MessageType.Image)
    {
      _chatRenderer.DisplayImage(messageOwner, e.Message.Content.BytesToImage(), false);
      return;
    }

    _chatRenderer.DisplayMessage(messageOwner, Encoding.UTF8.GetString(e.Message.Content), false);
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
    var messageOrFilePath = _messageTextbox.Text.Trim();
    if (string.IsNullOrEmpty(messageOrFilePath)) return;

    var broadcastTask = _isSendingImage ?
      BroadcastImageAsync(messageOrFilePath) :
      BroadcastMessageAsync(messageOrFilePath);
    await broadcastTask;

    Action displayingFunc = _isSendingImage ?
      () => _chatRenderer.DisplayImage("Server", Image.FromFile(messageOrFilePath), true) :
      () => _chatRenderer.DisplayMessage("Server", messageOrFilePath, true);

    displayingFunc();
    ClearServerMessageInput();
  }

  private async Task BroadcastMessageAsync(string content)
  {
    var bytes = Encoding.UTF8.GetBytes(content);
    await BroadcastMessageAsync(bytes, MessageType.Text);
  }

  private async Task BroadcastImageAsync(string filePath)
  {
    var image = Image.FromFile(filePath);
    var bytes = ImageByteConverter.ImageToBytes(image);

    await BroadcastMessageAsync(bytes, MessageType.Image);
  }

  private async Task BroadcastMessageAsync(byte[] content, MessageType messageType)
  {
    var clientInfo = new ClientInfo("Server");
    var message = new Core.Models.Message(clientInfo, content, messageType);

    await _server.BroadcastMessageToAllClientsAsync(message);
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
