using ChattingApplication.Common.Enums;
using ChattingApplication.Common.Events;
using ChattingApplication.Core.Interfaces;
using ChattingApplication.Core.Models;
using System.Text.Json;

namespace ChattingApplication;

public partial class ClientOnlineClientsForm : Form
{
  private static readonly ClientInfo SERVER_INFO = new("0", "Server");
  private readonly ListView _lvOnlineClients;
  private readonly System.Windows.Forms.Timer _timerUpdateClients;

  public ClientOnlineClientsForm(ClientInfo senderInfo, IClient client)
  {
    InitializeComponent();

    _lvOnlineClients = lvOnlineClients;
    _timerUpdateClients = timerUpdateClientsInfo;

    _lvOnlineClients.View = View.Details;
    _lvOnlineClients.FullRowSelect = true;
    _lvOnlineClients.MultiSelect = false;
    _lvOnlineClients.Columns.Add("Online Clients", -2, HorizontalAlignment.Left);

    client.UnicastMessageReceivedEventHandler += OnUnicastMessageReceived;

    _timerUpdateClients.Tick += (s, e) =>
      _ = client.SendMessageAsync(
        new Core.Models.Message(
          client.ClientInfo, [], MessageType.Any, Target.Server, MessageRequest.GetClientsInfo));

    _timerUpdateClients.Start();

    _lvOnlineClients.DoubleClick += (s, e) => OnClientDoubleClick(senderInfo, client);
  }
  private void OnUnicastMessageReceived(object? sender, MessageReceivedEventArgs e)
  {
    if (e.Message.Type is not MessageType.ActiveClientsInfo) return;

    var clientsInfo = JsonSerializer.Deserialize<IEnumerable<ClientInfo>>(e.Message.Content);
    DisplayOnlineClients(clientsInfo!);
  }

  private void DisplayOnlineClients(IEnumerable<ClientInfo> clientsInfo)
  {
    _lvOnlineClients.Items.Clear();
    _lvOnlineClients.Items.Add(new ListViewItem("Server"));

    foreach (var clientInfo in clientsInfo)
    {
      var item = new ListViewItem($"{clientInfo.Name} {clientInfo.Id}")
      {
        Tag = clientInfo
      };
      _lvOnlineClients.Items.Add(item);
    }
  }

  private void OnClientDoubleClick(ClientInfo senderInfo, IClient client)
  {
    if (_lvOnlineClients.SelectedItems.Count == 0) return;

    var selectedItem = _lvOnlineClients.SelectedItems[0];

    if (_lvOnlineClients.Items.IndexOf(selectedItem) == 0)
    {
      new ClientDirectMessageForm(senderInfo, SERVER_INFO, client).Show();
      return;
    }

    var selectedRecipient = (ClientInfo)selectedItem.Tag!;
    new ClientDirectMessageForm(senderInfo, selectedRecipient, client).Show();
  }
}
