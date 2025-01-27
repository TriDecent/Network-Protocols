using System.Text;
using ChattingApplication.Common.Enums;
using ChattingApplication.Common.Utils;
using ChattingApplication.Core.Interfaces;
using ChattingApplication.Core.Models;

namespace ChattingApplication
{
  public partial class ServerDirectMessageForm : Form
  {
    private readonly Label _clientNameLabel;

    private readonly Button _attachItemButton, _detachItemButton, _sendButton;
    private readonly TextBox _messageTextBox;
    private readonly RichTextBox _chatDisplayArea;
    private readonly ChatMessageRenderer _chatRenderer;

    private bool _isSendingImage = false;

    public ServerDirectMessageForm(IServer server, ClientSessionInfo clientInfo)
    {
      InitializeComponent();

      _clientNameLabel = lblClientName;
      _sendButton = btnSend;
      _attachItemButton = btnAttach;
      _detachItemButton = btnDetach;
      _messageTextBox = txtMessage;
      _chatDisplayArea = rtbDialogArea;
      _chatRenderer = new ChatMessageRenderer(_chatDisplayArea);

      _clientNameLabel.Text = clientInfo.Info.Name;
      _sendButton.Click += async (s, e) => await OnSendMessageClickedAsync(clientInfo, server);
    }

    private async Task OnSendMessageClickedAsync(ClientSessionInfo clientInfo, IServer server)
    {
      var message = _messageTextBox.Text.Trim();

      if (message == string.Empty) return;

      var sendTask = _isSendingImage ?
        SendImageAsync(message, Target.Individual, clientInfo, server) :
        SendMessageAsync(message, Target.Individual, clientInfo, server);

      await sendTask;

      ClearUserMessageInput();
    }

    private async Task SendImageAsync(
      string filePath,
      Target target,
      ClientSessionInfo clientInfo,
      IServer server)
    {
      var image = Image.FromFile(filePath);
      var bytes = ImageByteConverter.ImageToBytes(image);

      await SendMessageAsync(bytes, MessageType.Image, target, clientInfo, server);

      _chatRenderer.DisplayImage("Server", image, true);
    }

    private async Task SendMessageAsync(
      string text,
      Target target,
      ClientSessionInfo clientInfo,
      IServer server)
    {
      var bytes = Encoding.UTF8.GetBytes(text);

      await SendMessageAsync(bytes, MessageType.Text, target, clientInfo, server);

      _chatRenderer.DisplayMessage("Server", text, true);
    }

    private static async Task SendMessageAsync(
      byte[] content,
      MessageType messageType,
      Target target,
      ClientSessionInfo clientInfo,
      IServer server)
    {
      var message = new Core.Models.Message(clientInfo.Info, content, messageType, target);
      await server.SendUnicastMessageAsync(clientInfo, message);
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
