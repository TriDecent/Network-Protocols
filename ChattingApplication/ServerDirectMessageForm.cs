using ChattingApplication.Common.Enums;
using ChattingApplication.Common.Events;
using ChattingApplication.Common.Utils;
using ChattingApplication.Core.Models;
using ChattingApplication.Infrastructure.Network.Server.EventEmitter;
using ChattingApplication.Infrastructure.Network.Server.Operations;
using System.Text;
using Message = ChattingApplication.Core.Models.Message;

namespace ChattingApplication;

public partial class ServerDirectMessageForm : Form
{
  private static readonly ClientInfo SERVER_INFO = new("0", "Server");
  private readonly Label _clientNameLabel;

  private readonly Button _attachItemButton, _detachItemButton, _sendButton;
  private readonly TextBox _messageTextBox;
  private readonly RichTextBox _chatDisplayArea;
  private readonly ChatMessageRenderer _chatRenderer;

  private bool _isSendingImage = false;

  private readonly string _interactingClientId;

  public ServerDirectMessageForm(
    IServerOperations server,
    IServerEventEmitter eventEmitter,
    ClientSessionInfo recipient)
  {
    InitializeComponent();

    _clientNameLabel = lblClientName;
    _sendButton = btnSend;
    _attachItemButton = btnAttach;
    _detachItemButton = btnDetach;
    _messageTextBox = txtMessage;
    _chatDisplayArea = rtbDialogArea;
    _chatRenderer = new ChatMessageRenderer(_chatDisplayArea);
    _interactingClientId = recipient.Info.Id;

    _clientNameLabel.Text = recipient.Info.Name;
    eventEmitter.UnicastMessageReceived += OnUnicastMessageReceived;
    _sendButton.Click += async (s, e) => await OnSendMessageClickedAsync(recipient, server);
  }

  private async Task OnSendMessageClickedAsync(ClientSessionInfo recipient, IServerOperations server)
  {
    if (string.IsNullOrWhiteSpace(_messageTextBox.Text)) return;

    await SendContent(recipient, _messageTextBox.Text, server);
    ClearUserMessageInput();
  }

  private void OnUnicastMessageReceived(object? sender, MessageReceivedEventArgs e)
  {
    if (e.Message.Type is MessageType.ActiveClientsInfo) return;
    if (_interactingClientId != e.Message.Sender.Id) return;

    var messageOwner = e.Message.Sender.Name;

    if (e.Message.Type == MessageType.Image)
    {
      _chatRenderer.DisplayImage(messageOwner, e.Message.Content.BytesToImage(), false);
      return;
    }

    _chatRenderer.DisplayMessage(messageOwner, Encoding.UTF8.GetString(e.Message.Content), false);
  }

  private Task SendContent(ClientSessionInfo recipient, string content, IServerOperations server)
      => _isSendingImage ?
      SendImageAsync(recipient, content, server) :
      SendTextAsync(recipient, content, server);

  private async Task SendImageAsync(ClientSessionInfo recipient, string filePath, IServerOperations server)
  {
    using var image = Image.FromFile(filePath);
    var message = CreateMessage(
      ImageByteConverter.ImageToBytes(image),
      MessageType.Image);

    await SendAndDisplayAsync(recipient, message, server, () =>
      _chatRenderer.DisplayImage("Server", image, true));
  }

  private async Task SendTextAsync(ClientSessionInfo recipient, string text, IServerOperations server)
  {
    var message = CreateMessage(
      Encoding.UTF8.GetBytes(text),
      MessageType.Text);

    await SendAndDisplayAsync(recipient, message, server, () =>
      _chatRenderer.DisplayMessage("Server", text, true));
  }

  private static Message CreateMessage(byte[] content, MessageType type)
    => new(
      SERVER_INFO,
      content,
      type,
      Target.Individual,
      MessageRequest.None);

  private static async Task SendAndDisplayAsync(
    ClientSessionInfo recipient,
    Message message,
    IServerOperations server,
    Action displayAction)
  {
    await server.SendUnicastMessageAsync(recipient, message);
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
