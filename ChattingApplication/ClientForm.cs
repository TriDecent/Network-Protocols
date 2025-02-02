using ChattingApplication.Common.Enums;
using ChattingApplication.Common.Events;
using ChattingApplication.Common.Utils;
using ChattingApplication.Infrastructure.Network.Client;
using ChattingApplication.Infrastructure.Network.Client.EventEmitter;
using System.Net;
using System.Text;

using Message = ChattingApplication.Core.Models.Message;

namespace ChattingApplication;

public partial class ClientForm : Form
{
  private readonly Button _connectServerButton;
  private readonly Button _disconnectServerButton;
  private readonly Button _attachItemButton;
  private readonly Button _detachItemButton;
  private readonly Button _directMessageButton;
  private readonly RichTextBox _chatDisplayArea;
  private readonly TextBox _messageTextBox;
  private readonly TextBox _serverIPTextBox, _serverPortTextBox;
  private readonly TextBox _clientNameTextBox;
  private readonly Label _stateLabel;
  private readonly Client _client; // only for better performance
  private readonly IClientEventEmitter _eventEmitter;
  private readonly ChatMessageRenderer _chatRenderer;

  private bool _isSendingImage = false;
  private ClientOnlineClientsForm? _onlineClientsForm;

  public ClientForm(Client client, IClientEventEmitter eventEmitter)
  {
    InitializeComponent();

    _connectServerButton = btnConnectServer;
    _disconnectServerButton = btnDisconnectServer;
    _attachItemButton = btnAttach;
    _detachItemButton = btnDetach;
    _directMessageButton = btnDirectMessage;
    _messageTextBox = txtMessage;
    _chatDisplayArea = rtbDialogArea;
    _stateLabel = lblState;
    _serverIPTextBox = txtServerIP;
    _serverPortTextBox = txtServerPort;
    _clientNameTextBox = txtName;

    _chatRenderer = new ChatMessageRenderer(_chatDisplayArea);

    _client = client;
    _eventEmitter = eventEmitter;

    _eventEmitter.StateChanged += OnStateChanged;
    _eventEmitter.BroadcastMessageReceived += OnBroadcastMessageReceived;

    FormClosing += (s, e) => _client?.Dispose();

    _serverPortTextBox.KeyPress += (s, e) =>
    {
      if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
      {
        e.Handled = true;
      }
    };

    _serverPortTextBox.TextChanged += (s, e) =>
      EnableConnectButtonBasedOnServerInput();

    _serverIPTextBox.TextChanged += (s, e) =>
      EnableConnectButtonBasedOnServerInput();

    _directMessageButton.Click += (s, e) =>
    {
      if (_onlineClientsForm?.IsDisposed == false)
      {
        _onlineClientsForm.Focus();
        return;
      }

      _onlineClientsForm = new ClientOnlineClientsForm(
        _client.ClientInfo,
        _client,
        _client,
        _eventEmitter);

      _onlineClientsForm.Show();
    };
  }

  private void EnableConnectButtonBasedOnServerInput()
  {
    _connectServerButton.Enabled =
      IPAddressValidator.IsValidIP(_serverIPTextBox.Text.Trim()) &&
      IPAddressValidator.IsValidPort(_serverPortTextBox.Text.Trim());
  }

  private void OnStateChanged(object? sender, StateChangedEventArgs e)
  {
    var clientState = e.ClientState;

    _stateLabel.Text = $"State: {clientState}";

    bool isDisconnectedOrFailed = clientState == ClientState.Disconnected ||
      clientState == ClientState.Failed;
    bool isConnectingOrDisconnecting = clientState == ClientState.Connecting ||
      clientState == ClientState.Disconnecting;
    bool isConnected = clientState == ClientState.Connected;

    _serverIPTextBox.Enabled = isDisconnectedOrFailed;
    _serverPortTextBox.Enabled = isDisconnectedOrFailed;
    _clientNameTextBox.Enabled = isDisconnectedOrFailed;

    _connectServerButton.Enabled = !isConnectingOrDisconnecting;
    _disconnectServerButton.Enabled = !isConnectingOrDisconnecting;

    _connectServerButton.Visible = !isConnected;
    _disconnectServerButton.Visible = isConnected;

    _directMessageButton.Visible = isConnected;

    if (clientState != ClientState.Disconnected) return;
    _onlineClientsForm?.Close();
  }

  private void OnBroadcastMessageReceived(object? sender, MessageReceivedEventArgs e)
  {
    var messageOwner = e.Message.Sender.Name;

    if (e.Message.Type == MessageType.Image)
    {
      _chatRenderer.DisplayImage(messageOwner, e.Message.Content.BytesToImage(), false);
      return;
    }

    _chatRenderer.DisplayMessage(messageOwner, Encoding.UTF8.GetString(e.Message.Content), false);
  }

  private async void BtnConnectToServer_Click(object sender, EventArgs e)
  {
    var ip = IPAddress.Parse(_serverIPTextBox.Text);
    var port = int.Parse(_serverPortTextBox.Text);
    var ipEndPoint = new IPEndPoint(ip, port);

    var clientName = _clientNameTextBox.Text;

    if (clientName == string.Empty) clientName = "client";

    _client.UpdateName(clientName);

    var establishConnectionTask = await _client.ConnectServerAsync(ipEndPoint);

    if (establishConnectionTask.Success) return;

    MessageBox.Show(establishConnectionTask.ErrorMessage, "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
  }

  private void BtnDisconnectServer_Click(object sender, EventArgs e)
    => _client.DisconnectFromServer();

  private async void BtnSend_Click(object sender, EventArgs e)
  {
    if (_client.State != ClientState.Connected ||
        string.IsNullOrWhiteSpace(_messageTextBox.Text)) return;

    await SendContentAsync(_messageTextBox.Text);
    ClearUserMessageInput();
  }

  private Task SendContentAsync(string content)
      => _isSendingImage ? SendImageAsync(content) : SendTextAsync(content);

  private async Task SendImageAsync(string filePath)
  {
    using var image = Image.FromFile(filePath);
    var message = CreateMessage(
        ImageByteConverter.ImageToBytes(image),
        MessageType.Image);

    await SendAndDisplayAsync(message, () =>
      _chatRenderer.DisplayImage(_client.ClientInfo.Name, image, true));
  }

  private async Task SendTextAsync(string text)
  {
    var message = CreateMessage(
        Encoding.UTF8.GetBytes(text),
        MessageType.Text);

    await SendAndDisplayAsync(message, () =>
        _chatRenderer.DisplayMessage(_client.ClientInfo.Name, text, true));
  }

  private Message CreateMessage(byte[] content, MessageType type)
    => new(
      _client.ClientInfo,
      content,
      type,
      Target.All,
      MessageRequest.None);

  private async Task SendAndDisplayAsync(Message message, Action displayAction)
  {
    await _client.SendMessageAsync(message);
    displayAction();
  }

  private void BtnAttach_Click(object sender, EventArgs e)
  {
    using var openFileDialog = new OpenFileDialog
    {
      Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif"
    };

    if (openFileDialog.ShowDialog() == DialogResult.OK)
    {
      _messageTextBox.Text = openFileDialog.FileName;
      _isSendingImage = true;
      _messageTextBox.Enabled = false;
      ToggleAttachDetachButtons();
    }
  }

  private void BtnDetach_Click(object sender, EventArgs e)
  {
    _isSendingImage = false;
    _messageTextBox.Enabled = true;
    ClearUserMessageInput();
    ToggleAttachDetachButtons();
  }

  private void ToggleAttachDetachButtons()
  {
    _attachItemButton.Visible = !_isSendingImage;
    _detachItemButton.Visible = _isSendingImage;
  }

  private void ClearUserMessageInput() => _messageTextBox.Text = "";
}