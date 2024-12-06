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

      DisplayImage("Server", (byte[])e.Content, false);
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

      // Set alignment and padding
      _chatDisplayArea.SelectionAlignment = isOwnMessage ?
          HorizontalAlignment.Right : HorizontalAlignment.Left;

      // Add padding
      _chatDisplayArea.SelectionIndent = 10; // Left padding
      _chatDisplayArea.SelectionRightIndent = 10; // Right padding

      // Add timestamp and sender with appropriate colors
      _chatDisplayArea.SelectionColor = Color.Gray;
      _chatDisplayArea.AppendText($"[{DateTime.Now:HH:mm}] ");

      _chatDisplayArea.SelectionColor = isOwnMessage ?
          Color.Green : Color.Blue;
      _chatDisplayArea.AppendText($"{sender}: ");

      // Add message with background
      _chatDisplayArea.SelectionColor = Color.Black;
      _chatDisplayArea.SelectionBackColor = isOwnMessage ?
          Color.FromArgb(220, 248, 198) : Color.FromArgb(200, 235, 255);
      _chatDisplayArea.AppendText($"{message}{Environment.NewLine}{Environment.NewLine}");

      // Reset padding and caret
      _chatDisplayArea.SelectionIndent = 0;
      _chatDisplayArea.SelectionRightIndent = 0;
      _chatDisplayArea.SelectionStart = _chatDisplayArea.TextLength;
      _chatDisplayArea.ScrollToCaret();
    }

    private void DisplayImage(string sender, byte[] bytes, bool isOwnMessage = false)
    {
      var image = bytes.BytesToImage();

      // Resize image if needed
      if (image.Width > _chatDisplayArea.ClientSize.Width - 40)
      {
        float ratio = (float)(_chatDisplayArea.ClientSize.Width - 40) / image.Width;
        int newWidth = (int)(image.Width * ratio);
        int newHeight = (int)(image.Height * ratio);
        image = new Bitmap(image, new Size(newWidth, newHeight));
      }

      // Start new paragraph
      _chatDisplayArea.SelectionStart = _chatDisplayArea.TextLength;
      _chatDisplayArea.SelectionLength = 0;

      // Add padding
      _chatDisplayArea.SelectionIndent = isOwnMessage ? 50 : 10; // Padding left for sent/received
      _chatDisplayArea.SelectionRightIndent = isOwnMessage ? 10 : 50; // Padding right for sent/received

      // Set alignment for the header and image
      _chatDisplayArea.SelectionAlignment = isOwnMessage ?
          HorizontalAlignment.Right : HorizontalAlignment.Left;

      // Add header
      _chatDisplayArea.SelectionColor = Color.Gray;
      _chatDisplayArea.AppendText($"[{DateTime.Now:HH:mm}] ");
      _chatDisplayArea.SelectionColor = isOwnMessage ? Color.Green : Color.Blue;
      _chatDisplayArea.AppendText($"{sender}{Environment.NewLine}");

      // Insert image
      _chatDisplayArea.ReadOnly = false;
      Clipboard.SetImage(image);
      _chatDisplayArea.Paste();
      _chatDisplayArea.ReadOnly = true;

      // Add "Sent an image" text
      _chatDisplayArea.AppendText(Environment.NewLine); // Add spacing
      _chatDisplayArea.SelectionColor = Color.BlueViolet;
      _chatDisplayArea.SelectionBackColor = Color.Transparent; // Transparent background
      _chatDisplayArea.AppendText("Sent an image");
      _chatDisplayArea.AppendText(Environment.NewLine + Environment.NewLine); // Add additional spacing

      // Reset padding and alignment
      _chatDisplayArea.SelectionIndent = 0;
      _chatDisplayArea.SelectionRightIndent = 0;
      _chatDisplayArea.SelectionAlignment = HorizontalAlignment.Left; // Reset alignment to default
      _chatDisplayArea.ScrollToCaret();
    }
  }
}
