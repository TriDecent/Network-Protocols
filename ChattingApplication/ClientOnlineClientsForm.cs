using ChattingApplication.Common.Enums;
using ChattingApplication.Common.Events;
using ChattingApplication.Core.Interfaces;
using ChattingApplication.Core.Models;
using ChattingApplication.Infrastructure.Network.Client.EventEmitter;
using ChattingApplication.Infrastructure.Network.Client.Operations;
using System.Text.Json;
using Message = ChattingApplication.Core.Models.Message;

namespace ChattingApplication;

public partial class ClientOnlineClientsForm : Form
{
  private static readonly ClientInfo SERVER_INFO = new("0", "Server");
  private readonly ListView _lvOnlineClients;
  private readonly System.Windows.Forms.Timer _timerUpdateClients;

  private readonly EventHandler _timerTickHandler;
  private readonly EventHandler<MessageReceivedEventArgs> _messageReceivedHandler;

  public ClientOnlineClientsForm(
    ClientInfo senderInfo,
    IClient client,
    IClientOperations operations,
    IClientEventEmitter eventEmitter)
  {
    InitializeComponent();

    _lvOnlineClients = lvOnlineClients;
    _timerUpdateClients = timerUpdateClientsInfo;

    _lvOnlineClients.View = View.Details;
    _lvOnlineClients.FullRowSelect = true;
    _lvOnlineClients.MultiSelect = false;
    _lvOnlineClients.Columns.Add("Online Clients", -2, HorizontalAlignment.Left);

    _messageReceivedHandler = OnUnicastMessageReceived;
    eventEmitter.UnicastMessageReceived += _messageReceivedHandler;

    _timerTickHandler += (s, e) =>
      _ = operations.SendMessageAsync(
        new Message(
          client.ClientInfo,
          [],
          MessageType.Any,
          Target.Server,
          MessageRequest.GetClientsInfo));
    _timerUpdateClients.Tick += _timerTickHandler;

    _timerUpdateClients.Start();

    _lvOnlineClients.DoubleClick += (s, e)
      => OnClientDoubleClick(senderInfo, operations, eventEmitter);

    FormClosing += (s, e) => CleanupHandlers(eventEmitter);
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

  private void OnClientDoubleClick(
    ClientInfo senderInfo,
    IClientOperations operations,
    IClientEventEmitter eventEmitter)
  {
    if (_lvOnlineClients.SelectedItems.Count == 0) return;

    var selectedItem = _lvOnlineClients.SelectedItems[0];

    if (_lvOnlineClients.Items.IndexOf(selectedItem) == 0)
    {
      new ClientDirectMessageForm(
        senderInfo,
        SERVER_INFO,
        operations,
        eventEmitter).Show();
      return;
    }

    var selectedRecipient = (ClientInfo)selectedItem.Tag!;
    new ClientDirectMessageForm(
      senderInfo,
      selectedRecipient,
      operations,
      eventEmitter).Show();
  }

  private void CleanupHandlers(IClientEventEmitter eventEmitter)
  {
    _timerUpdateClients.Stop();
    _timerUpdateClients.Tick -= _timerTickHandler;
    eventEmitter.UnicastMessageReceived -= _messageReceivedHandler;
  }
}
