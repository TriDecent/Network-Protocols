using System.Net;
using System.Net.Sockets;
using ChattingApplication.Enums;
using ChattingApplication.Events;
using ChattingApplication.Utils;

namespace ChattingApplication
{
  public partial class ClientForm : Form
  {
    private readonly Button _connectServerButton;
    private readonly Button _disconnectServerButton;
    private readonly Button _sendMessageButton;
    private readonly Button _attachItemButton;
    private readonly Button _detachItemButton;
    private readonly RichTextBox _chatDisplayArea;
    private readonly TextBox _messageTextBox;
    private readonly Label _statusLabel;
    private readonly Client _client = new(new TcpClient());
    private readonly ChatMessageRenderer _chatRenderer;

    private bool _isSendingImage = false;

    public ClientForm()
    {
      InitializeComponent();

      _connectServerButton = btnConnectServer;
      _disconnectServerButton = btnDisconnectServer;
      _sendMessageButton = btnSend;
      _attachItemButton = btnAttach;
      _detachItemButton = btnDetach;
      _messageTextBox = txtMessage;
      _chatDisplayArea = rtbDialogArea;
      _statusLabel = lblStatus;

      _chatRenderer = new ChatMessageRenderer(_chatDisplayArea);

      _client.StatusChangedEventHandler += OnStatusChanged;
      _client.MessageReceivedEventHandler += OnMessageReceived;
    }

    private void OnStatusChanged(object? sender, StateChangedEventArgs e)
    {
      var clientState = e.ClientState;

      _statusLabel.Text = $"State: {clientState}";

      _connectServerButton.Enabled = clientState != ClientState.Connecting
        && clientState != ClientState.Disconnecting;
      _disconnectServerButton.Enabled = _connectServerButton.Enabled;

      _connectServerButton.Visible = clientState != ClientState.Connected;
      _disconnectServerButton.Visible = !_connectServerButton.Visible;
    }

    private void OnMessageReceived(object? sender, MessageReceivedEventArgs e)
    {
      if (e.Type == MessageType.Text)
      {
        _chatRenderer.DisplayMessage("Server", (string)e.Content);
        return;
      }

      var bytes = e.Content as byte[] ?? [];
      _chatRenderer.DisplayImage("Server", bytes.BytesToImage(), false);
    }

    private async void BtnConnectToServer_Click(object sender, EventArgs e)
    {
      var ip = IPAddress.Parse("192.168.84.128");
      var port = 1211;
      var ipEndPoint = new IPEndPoint(ip, port);

      try
      {
        await _client.ConnectServerAsync(ipEndPoint);
      }
      catch (SocketException ex)
      {
        string errorMessage = ex.SocketErrorCode switch
        {
          SocketError.ConnectionRefused =>
          "Could not connect to the server. " + "Please try again later.",

          SocketError.HostUnreachable or SocketError.NetworkUnreachable =>
            "The server is not reachable. " +
              "Please check your internet connection and try again.",

          _ => "An unexpected error occurred. " +
            "Please try again later.",
        };
        MessageBox.Show(errorMessage, "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void BtnDisconnectServer_Click(object sender, EventArgs e)
      => _client.DisconnectFromServer();

    private async void BtnSend_Click(object sender, EventArgs e)
    {
      if (_client.State != ClientState.Connected) return;

      var message = _messageTextBox.Text.Trim();

      if (message == string.Empty) return;

      try
      {
        if (_isSendingImage)
        {
          var filePath = _messageTextBox.Text;
          var image = Image.FromFile(filePath);

          await _client.SendImageAsync(image);

          _chatRenderer.DisplayImage("You", image, true);

          return;
        }

        await _client.SendMessageAsync(message);

        _chatRenderer.DisplayMessage("You", message, true);

        ClearUserMessageInput();
      }
      catch (IOException ex)
      {
        MessageBox.Show(ex.Message, "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
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

    private void ClearUserMessageInput()
      => _messageTextBox.Text = "";
  }
}