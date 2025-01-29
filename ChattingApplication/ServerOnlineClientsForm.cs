using ChattingApplication.Common.Events;
using ChattingApplication.Core.Interfaces;
using ChattingApplication.Core.Models;

namespace ChattingApplication;

public partial class ServerOnlineClientsForm : Form
{
  private readonly ListView _lvOnlineClients;

  public ServerOnlineClientsForm(IServer server)
  {
    InitializeComponent();
    _lvOnlineClients = lvOnlineClients;

    _lvOnlineClients.View = View.Details;
    _lvOnlineClients.FullRowSelect = true;
    _lvOnlineClients.MultiSelect = false;
    _lvOnlineClients.Columns.Add("Online Clients", -2, HorizontalAlignment.Left);

    _lvOnlineClients.DoubleClick += (s, e) => OnClientDoubleClick(server);

    var clientsInfo = server.ClientsInfo;
    DisplayClientsInfo(clientsInfo);

    server.ClientConnectedEventHandler += OnConnectedClient;
    server.ClientDisconnectedEventHandler += OnDisconnectedClient;
  }

  private void DisplayClientsInfo(IEnumerable<ClientSessionInfo> clientsInfo)
  {
    _lvOnlineClients.Items.Clear();

    foreach (var clientInfo in clientsInfo)
    {
      var item = new ListViewItem(clientInfo.Info.Name)
      {
        Tag = clientInfo
      };
      _lvOnlineClients.Items.Add(item);
    }
  }

  private void OnConnectedClient(object? sender, ClientSessionInfoEventArgs e)
  {
    if (InvokeRequired)
    {
      Invoke(() => OnConnectedClient(sender, e));
      return;
    }

    var item = new ListViewItem($"{e.ClientSessionInfo.Info.Name} {e.ClientSessionInfo.Info.Id}")
    {
      Tag = e.ClientSessionInfo
    };
    _lvOnlineClients.Items.Add(item);
  }

  private void OnDisconnectedClient(object? sender, ClientSessionInfoEventArgs e)
  {
    if (InvokeRequired)
    {
      Invoke(() => OnDisconnectedClient(sender, e));
      return;
    }

    foreach (ListViewItem item in _lvOnlineClients.Items)
    {
      if (item.Tag is ClientSessionInfo client)
      {
        _lvOnlineClients.Items.Remove(item);
        break;
      }
    }
  }

  private void OnClientDoubleClick(IServer server)
  {
    if (_lvOnlineClients.SelectedItems.Count == 0) return;

    var selectedItem = _lvOnlineClients.SelectedItems[0];
    var clientInfo = (ClientSessionInfo)selectedItem.Tag!;

    new ServerDirectMessageForm(server, clientInfo).Show();
  }
}
