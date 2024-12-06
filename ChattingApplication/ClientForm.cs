using System.Net;
using System.Net.Sockets;

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

      _client.StatusEventHandler += OnStatusChanged;
    }

    private void OnStatusChanged(object? sender, StateEventArgs eventArgs)
    {
      var clientState = eventArgs.ClientState;

      _statusLabel.Text = $"State: {clientState}";

      if (clientState == ClientState.Connected)
      {
        _connectServerButton.Visible = false;
        _disconnectServerButton.Visible = true;

        return;
      }

      _connectServerButton.Visible = true;
      _disconnectServerButton.Visible = false;
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
    }

    private void BtnAttach_Click(object sender, EventArgs e)
    {

    }
  }
}
