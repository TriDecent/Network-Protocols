using ChattingApplication.Common.Enums;
using ChattingApplication.Common.Events;
using ChattingApplication.Common.Utils;
using ChattingApplication.Core.Models;
using ChattingApplication.Infrastructure.Network.Client.EventEmitter;
using ChattingApplication.Infrastructure.Network.Client.Operations;
using System.Text;
using System.Text.Json;
using Message = ChattingApplication.Core.Models.Message;
using Timer = System.Windows.Forms.Timer;

namespace ChattingApplication;

public partial class ClientDirectMessageForm : Form
{
  private readonly Label _recipientNameLabel, _senderNameLabel;

  private readonly Button _attachItemButton, _detachItemButton, _sendButton;
  private readonly TextBox _messageTextBox;
  private readonly RichTextBox _chatDisplayArea;
  private readonly ChatMessageRenderer _chatRenderer;

  private bool _isSendingImage = false;

  private readonly ClientInfo _sender;
  private readonly ClientInfo _recipient;

  private readonly Timer _activeRecipientCheckTimer;
  private readonly EventHandler _timerTickHandler;
  private EventHandler<MessageReceivedEventArgs>? _messageReceivedHandler;

  public ClientDirectMessageForm(
    ClientInfo sender,
    ClientInfo recipient,
    IClientOperations operations,
    IClientEventEmitter eventEmitter)
  {
    InitializeComponent();

    _sender = sender;
    _recipient = recipient;

    _recipientNameLabel = lblRecipientName;
    _senderNameLabel = lblSenderName;
    _sendButton = btnSend;
    _attachItemButton = btnAttach;
    _detachItemButton = btnDetach;
    _messageTextBox = txtMessage;
    _chatDisplayArea = rtbDialogArea;
    _activeRecipientCheckTimer = RecipientActivityCheckTimer;
    _chatRenderer = new ChatMessageRenderer(_chatDisplayArea);

    _recipientNameLabel.Text = recipient.Name;
    _senderNameLabel.Text = sender.Name;

    _sendButton.Click += async (s, e) =>
      await OnSendMessageClickedAsync(operations);
    _attachItemButton.Click += (s, e) => OnAttachButtonClicked();
    _detachItemButton.Click += (s, e) => OnDetachButtonClicked();

    _messageReceivedHandler = (s, e) => OnUnicastMessageReceived(e.Message);
    eventEmitter.UnicastMessageReceived += _messageReceivedHandler;

    _timerTickHandler = (s, e) =>
      _ = operations.SendMessageAsync(
        new Message(
          sender,
          [],
          MessageType.Any,
          Target.Server,
          MessageRequest.GetClientsInfo));


    _activeRecipientCheckTimer.Tick += _timerTickHandler;
    _activeRecipientCheckTimer.Start();

    FormClosing += (s, e) => CleanupHandlers(eventEmitter);
  }

  private async Task OnSendMessageClickedAsync(
    IClientOperations operations)
  {
    if (string.IsNullOrWhiteSpace(_messageTextBox.Text)) return;

    await SendContent(_messageTextBox.Text, operations);
    ClearUserMessageInput();
  }

  private void OnUnicastMessageReceived(
    Message message)
  {
    if (message.Type is MessageType.ActiveClientsInfo)
    {
      var clientsInfo = JsonSerializer.Deserialize<IEnumerable<ClientInfo>>(message.Content);
      var isRecipientActive = clientsInfo?.Any(client => client.Id == _recipient.Id) ?? false;
      if (!isRecipientActive)
      {
        MessageBox.Show(
          "The recipient is no longer active. " +
          "They cannot receive your message anymore.",
          "Recipient Inactive",
          MessageBoxButtons.OK,
          MessageBoxIcon.Warning);
        Close();
      }

      return;
    }

    if (message.Sender.Id != _recipient.Id) return;

    var messageOwner = message.Sender.Name;

    if (message.Type == MessageType.Image)
    {
      _chatRenderer.DisplayImage(
        messageOwner, message.Content.BytesToImage(), false);
      return;
    }

    _chatRenderer.DisplayMessage(
      messageOwner, Encoding.UTF8.GetString(message.Content), false);
  }

  private Task SendContent(
    string content,
    IClientOperations operations)
      => _isSendingImage ?
      SendImageAsync(content, operations) :
      SendTextAsync(content, operations);

  private async Task SendImageAsync(
    string filePath,
    IClientOperations operations)
  {
    using var image = Image.FromFile(filePath);
    var message = CreateMessage(
      _sender,
      ImageByteConverter.ImageToBytes(image),
      MessageType.Image,
      _recipient);

    await SendAndDisplayAsync(message, operations, () =>
      _chatRenderer.DisplayImage(_sender.Name, image, true));
  }

  private async Task SendTextAsync(
    string text,
    IClientOperations operations)
  {
    var message = CreateMessage(
      _sender,
      Encoding.UTF8.GetBytes(text),
      MessageType.Text,
      _recipient);

    await SendAndDisplayAsync(message, operations, () =>
      _chatRenderer.DisplayMessage(_sender.Name, text, true));
  }

  private static Message CreateMessage(
    ClientInfo sender, byte[] content, MessageType type, ClientInfo recipient)
      => new(
        sender,
        content,
        type,
        Target.Individual,
        MessageRequest.None,
        recipient);

  private static async Task SendAndDisplayAsync(
    Message message,
    IClientOperations operation,
    Action displayAction)
  {
    await operation.SendMessageAsync(message);
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

  private void CleanupHandlers(IClientEventEmitter eventEmitter)
  {
    _activeRecipientCheckTimer.Stop();
    _activeRecipientCheckTimer.Tick -= _timerTickHandler;
    eventEmitter.UnicastMessageReceived -= _messageReceivedHandler;
  }
}
