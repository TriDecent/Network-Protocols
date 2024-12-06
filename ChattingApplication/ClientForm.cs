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
    private readonly RichTextBox _chatDisplayArea;
    private readonly TextBox _messageTextBox;
    private readonly Label _statusLabel;
    private readonly Client _client = new(new TcpClient());

    public ClientForm()
    {
      InitializeComponent();

      _connectServerButton = btnConnectServer;
      _disconnectServerButton = btnDisconnectServer;
      _sendMessageButton = btnSend;
      _attachItemButton = btnAttach;
      _messageTextBox = txtMessage;
      _chatDisplayArea = rtbDialogArea;
      _statusLabel = lblStatus;

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
        DisplayMessage("Server", (string)e.Content);
        return;
      }

      DisplayImage((byte[])e.Content);
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
        string errorMessage;
        switch (ex.SocketErrorCode)
        {
          case SocketError.ConnectionRefused:
            errorMessage = "Could not connect to the server. " +
              "Please try again later.";
            break;
          case SocketError.HostUnreachable:
          case SocketError.NetworkUnreachable:
            errorMessage = "The server is not reachable. " +
              "Please check your internet connection and try again.";
            break;
          default:
            errorMessage = "An unexpected error occurred. " +
              "Please try again later.";
            break;
        }
        MessageBox.Show(errorMessage, "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void BtnDisconnectServer_Click(object sender, EventArgs e)
      => _client.DisconnectFromServer();

    private void BtnSend_Click(object sender, EventArgs e)
    {
      if (_client.State != ClientState.Connected) return;

      var message = _messageTextBox.Text.Trim();

      if (message == string.Empty) return;

      _client.SendMessage(message);

      DisplayMessage("You", message, true);
    }

    private void BtnAttach_Click(object sender, EventArgs e)
    {
      var openFileDialog = new OpenFileDialog
      {
        Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif"
      };

      if (openFileDialog.ShowDialog() == DialogResult.OK)
      {
        string filePath = openFileDialog.FileName;
      }
    }

    private void DisplayMessage(string sender, string message, bool isOwnMessage = false)
    {
      // Create a new paragraph
      _chatDisplayArea.SelectionStart = _chatDisplayArea.TextLength;
      _chatDisplayArea.SelectionLength = 0;

      // Set alignment
      _chatDisplayArea.SelectionAlignment = isOwnMessage ?
        HorizontalAlignment.Right : HorizontalAlignment.Left;

      // Add timestamp and sender with appropriate colors
      _chatDisplayArea.SelectionColor = Color.Gray;
      _chatDisplayArea.AppendText($"[{DateTime.Now:HH:mm}] ");

      _chatDisplayArea.SelectionColor = isOwnMessage ?
        Color.Green : Color.Blue;
      _chatDisplayArea.AppendText($"{sender}: ");

      // Add message with different background for own messages
      _chatDisplayArea.SelectionColor = Color.Black;
      _chatDisplayArea.SelectionBackColor = isOwnMessage ?
        Color.FromArgb(220, 248, 198) : Color.White;
      _chatDisplayArea.AppendText($"{message}{Environment.NewLine}");

      // Reset the background color and scroll to the caret.
      _chatDisplayArea.SelectionBackColor = Color.White;
      _chatDisplayArea.ScrollToCaret();
    }

    private void DisplayImage(byte[] bytes)
    {
      // var image = bytes.BytesToImage();
      // _chatDisplayArea.Paste(image);
    }
  }
}
