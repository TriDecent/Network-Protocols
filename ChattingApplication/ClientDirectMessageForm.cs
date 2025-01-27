using System.Text;
using ChattingApplication.Common.Enums;
using ChattingApplication.Common.Events;
using ChattingApplication.Common.Utils;
using ChattingApplication.Core.Interfaces;
using ChattingApplication.Core.Models;

namespace ChattingApplication
{
  public partial class ClientDirectMessageForm : Form
  {
    private readonly Label _clientNameLabel;

    private readonly Button _attachItemButton, _detachItemButton, _sendButton;
    private readonly TextBox _messageTextBox;
    private readonly RichTextBox _chatDisplayArea;
    private readonly ChatMessageRenderer _chatRenderer;

    private bool _isSendingImage = false;

    public ClientDirectMessageForm(ClientInfo recipient, IClient client)
    {
      InitializeComponent();

      _clientNameLabel = lblClientName;
      _sendButton = btnSend;
      _attachItemButton = btnAttach;
      _detachItemButton = btnDetach;
      _messageTextBox = txtMessage;
      _chatDisplayArea = rtbDialogArea;
      _chatRenderer = new ChatMessageRenderer(_chatDisplayArea);

      _clientNameLabel.Text = "server"; // temp
      _sendButton.Click += async (s, e) => await OnSendMessageClickedAsync(recipient, client);

      client.UnicastMessageReceivedEventHandler += OnUnicastMessageReceived;
    }

    private async Task OnSendMessageClickedAsync(ClientInfo recipient, IClient client)
    {
      if (string.IsNullOrWhiteSpace(_messageTextBox.Text)) return;

      await SendContent(recipient, _messageTextBox.Text, client);
      ClearUserMessageInput();
    }

    private void OnUnicastMessageReceived(object? sender, MessageReceivedEventArgs e)
    {
      if (e.Message.Type is MessageType.ActiveClientsInfo) return;

      var messageOwner = e.Message.Sender.Name;

      if (e.Message.Type == MessageType.Image)
      {
        _chatRenderer.DisplayImage(messageOwner, e.Message.Content.BytesToImage(), false);
        return;
      }

      _chatRenderer.DisplayMessage(messageOwner, Encoding.UTF8.GetString(e.Message.Content), false);
    }

    private Task SendContent(ClientInfo recipient, string content, IClient client)
        => _isSendingImage ?
        SendImageAsync(recipient, content, client) :
        SendTextAsync(recipient, content, client);

    private async Task SendImageAsync(ClientInfo recipient, string filePath, IClient client)
    {
      using var image = Image.FromFile(filePath);
      var message = CreateMessage(
        ImageByteConverter.ImageToBytes(image),
        MessageType.Image);

      await SendAndDisplayAsync(recipient, message, client, () =>
        _chatRenderer.DisplayImage("Server", image, true));
    }

    private async Task SendTextAsync(ClientInfo recipient, string text, IClient client)
    {
      var message = CreateMessage(
        Encoding.UTF8.GetBytes(text),
        MessageType.Text);

      await SendAndDisplayAsync(recipient, message, client, () =>
        _chatRenderer.DisplayMessage("Server", text, true));
    }

    private static Core.Models.Message CreateMessage(byte[] content, MessageType type)
      => new(
        new ClientInfo("Server"),
        content,
        type,
        Target.Individual,
        MessageRequest.None);


    // TODO: implement DM to a specific person, this version is communicating
    // to server only
    private static async Task SendAndDisplayAsync(
      ClientInfo recipient,
      Core.Models.Message message,
      IClient client,
      Action displayAction)
    {
      await client.SendMessageAsync(message);
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
}
