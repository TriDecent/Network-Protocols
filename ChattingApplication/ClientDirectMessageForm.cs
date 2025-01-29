using ChattingApplication.Common.Enums;
using ChattingApplication.Common.Events;
using ChattingApplication.Common.Utils;
using ChattingApplication.Core.Interfaces;
using ChattingApplication.Core.Models;
using ChattingApplication.Infrastructure.Network.Client;
using System.Text;

namespace ChattingApplication;

public partial class ClientDirectMessageForm : Form
{
  private readonly Label _recipientNameLabel, _senderNameLabel;

  private readonly Button _attachItemButton, _detachItemButton, _sendButton;
  private readonly TextBox _messageTextBox;
  private readonly RichTextBox _chatDisplayArea;
  private readonly ChatMessageRenderer _chatRenderer;

  private bool _isSendingImage = false;

  private readonly string _interactingId;

  public ClientDirectMessageForm(
    ClientInfo sender, ClientInfo recipient, IClient client, IClientEventEmitter eventEmitter)
  {
    InitializeComponent();

    _recipientNameLabel = lblRecipientName;
    _senderNameLabel = lblSenderName;
    _sendButton = btnSend;
    _attachItemButton = btnAttach;
    _detachItemButton = btnDetach;
    _messageTextBox = txtMessage;
    _chatDisplayArea = rtbDialogArea;
    _chatRenderer = new ChatMessageRenderer(_chatDisplayArea);

    _recipientNameLabel.Text = recipient.Name;
    _senderNameLabel.Text = sender.Name;

    _interactingId = recipient.Id;

    _sendButton.Click += async (s, e) =>
      await OnSendMessageClickedAsync(sender, recipient, client);
    _attachItemButton.Click += (s, e) => OnAttachButtonClicked();
    _detachItemButton.Click += (s, e) => OnDetachButtonClicked();

    eventEmitter.UnicastMessageReceived += OnUnicastMessageReceived;
  }

  private async Task OnSendMessageClickedAsync(ClientInfo sender, ClientInfo recipient, IClient client)
  {
    if (string.IsNullOrWhiteSpace(_messageTextBox.Text)) return;

    await SendContent(sender, _messageTextBox.Text, client, recipient);
    ClearUserMessageInput();
  }

  private void OnUnicastMessageReceived(object? sender, MessageReceivedEventArgs e)
  {
    if (e.Message.Type is MessageType.ActiveClientsInfo) return;
    if (e.Message.Sender.Id != _interactingId) return;

    var messageOwner = e.Message.Sender.Name;

    if (e.Message.Type == MessageType.Image)
    {
      _chatRenderer.DisplayImage(messageOwner, e.Message.Content.BytesToImage(), false);
      return;
    }

    _chatRenderer.DisplayMessage(messageOwner, Encoding.UTF8.GetString(e.Message.Content), false);
  }

  private Task SendContent(
    ClientInfo sender, string content, IClient client, ClientInfo recipient)
      => _isSendingImage ?
      SendImageAsync(sender, content, client, recipient) :
      SendTextAsync(sender, content, client, recipient);

  private async Task SendImageAsync(ClientInfo sender, string filePath, IClient client, ClientInfo recipient)
  {
    using var image = Image.FromFile(filePath);
    var message = CreateMessage(
      sender,
      ImageByteConverter.ImageToBytes(image),
      MessageType.Image,
      recipient);

    await SendAndDisplayAsync(message, client, () =>
      _chatRenderer.DisplayImage(sender.Name, image, true));
  }

  private async Task SendTextAsync(ClientInfo sender, string text, IClient client, ClientInfo recipient)
  {
    var message = CreateMessage(
      sender,
      Encoding.UTF8.GetBytes(text),
      MessageType.Text,
      recipient);

    await SendAndDisplayAsync(message, client, () =>
      _chatRenderer.DisplayMessage(sender.Name, text, true));
  }

  private static Core.Models.Message CreateMessage(
    ClientInfo sender, byte[] content, MessageType type, ClientInfo recipient)
      => new(
        sender,
        content,
        type,
        Target.Individual,
        MessageRequest.None,
        recipient);

  private static async Task SendAndDisplayAsync(
    Core.Models.Message message,
    IClient client,
    Action displayAction)
  {
    await client.SendMessageAsync(message);
    displayAction();
  }

  private void OnAttachButtonClicked()
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

  private void OnDetachButtonClicked()
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
