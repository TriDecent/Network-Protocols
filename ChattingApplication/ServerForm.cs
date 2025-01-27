using ChattingApplication.Common.Enums;
using ChattingApplication.Common.Events;
using ChattingApplication.Common.Utils;
using ChattingApplication.Core.Models;
using ChattingApplication.Infrastructure.Network;
using System.Diagnostics;
using System.Text;

namespace ChattingApplication;

public partial class ServerForm : Form
{
  private readonly Button _startServerButton, _stopServerButton, _shutdownButton;
  private readonly Button _attachButton, _detachButton;
  private readonly Button _dmButton;
  private readonly Label _stateLabel;
  private readonly Label _connectedClientsLabel;
  private readonly RichTextBox _chatDisplayArea;
  private readonly TextBox _messageTextbox;

  private readonly ChatMessageRenderer _chatRenderer;

  private readonly Server _server; // only for better performance

  private bool _isSendingImage;

  private static ServerOnlineClientsForm? _dmForm;

  public ServerForm(Server server)
  {
    InitializeComponent();

    _startServerButton = btnStart;
    _stopServerButton = btnStop;
    _shutdownButton = btnShutDown;
    _dmButton = btnDirectMessage;
    _stateLabel = lblState;
    _connectedClientsLabel = lblConnectedClients;
    _attachButton = btnAttach;
    _detachButton = btnDetach;
    _chatDisplayArea = rtbDialogArea;
    _messageTextbox = txtMessage;

    _chatRenderer = new ChatMessageRenderer(_chatDisplayArea);

    _server = server;

    _server.BroadcastMessageReceivedEventHandler += OnMessageReceived;
    _server.StateChangedEventHandler += OnStateChanged;
    _server.ClientsCountChangedEventHandler += (s, connectedClientsCount)
      => _connectedClientsLabel.Text = connectedClientsCount.ToString();

    _dmButton.Click += (s, e) =>
    {
      if (_dmForm is not null) return;

      _dmForm = new ServerOnlineClientsForm(_server);
      _dmForm.FormClosing += (s, e) => _dmForm = null;
      _dmForm.Show();
    };

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
    _dmButton.Visible = isServerActive;
  }

  private void OnMessageReceived(object? sender, MessageReceivedEventArgs e)
  {
    var messageOwner = e.Message.Sender.Name;

    if (e.Message.Type == MessageType.Image)
    {
      _chatRenderer.DisplayImage(messageOwner, e.Message.Content.BytesToImage());
      return;
    }

    _chatRenderer.DisplayMessage(messageOwner, Encoding.UTF8.GetString(e.Message.Content));
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
    if (string.IsNullOrWhiteSpace(_messageTextbox.Text)) return;

    await SendContent(_messageTextbox.Text);
    ClearServerMessageInput();
  }

  private Task SendContent(string content)
      => _isSendingImage ? SendImage(content) : SendText(content);

  private async Task SendImage(string filePath)
  {
    using var image = Image.FromFile(filePath);
    var message = CreateMessage(
        ImageByteConverter.ImageToBytes(image),
        MessageType.Image);

    await SendAndDisplay(message, () =>
        _chatRenderer.DisplayImage("Server", image, true));
  }

  private async Task SendText(string text)
  {
    var message = CreateMessage(
      Encoding.UTF8.GetBytes(text),
      MessageType.Text);

    await SendAndDisplay(message, () =>
      _chatRenderer.DisplayMessage("Server", text, true));
  }

  private static Core.Models.Message CreateMessage(byte[] content, MessageType type)
    => new(
      new ClientInfo("Server"),
      content,
      type,
      Target.All,
      MessageRequest.None);

  private async Task SendAndDisplay(Core.Models.Message message, Action displayAction)
  {
    await _server.BroadcastMessageToAllClientsAsync(message);
    displayAction();
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
