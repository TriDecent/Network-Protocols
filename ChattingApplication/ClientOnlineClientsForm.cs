using System.Text.Json;
using ChattingApplication.Common.Enums;
using ChattingApplication.Common.Events;
using ChattingApplication.Core.Interfaces;
using ChattingApplication.Core.Models;

namespace ChattingApplication
{
  public partial class ClientOnlineClientsForm : Form
  {
    private readonly ListView _lvOnlineClients;
    private readonly System.Windows.Forms.Timer _timerUpdateClients;

    public ClientOnlineClientsForm(IClient client)
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
            client.ClientDetails, [], MessageType.Any, Target.Server, MessageRequest.GetClientsInfo));
      
      _timerUpdateClients.Start();

      _lvOnlineClients.DoubleClick += (s, e) => OnClientDoubleClick(client);
    }
    private void OnUnicastMessageReceived(object? sender, MessageReceivedEventArgs e)
    {
      var message = e.Message;
      if (message.Type is MessageType.ActiveClientsInfo)
      {
        var clientsInfo = JsonSerializer.Deserialize<IEnumerable<ClientInfo>>(message.Content);
        DisplayOnlineClients(clientsInfo!);
      }
    }

    private void DisplayOnlineClients(IEnumerable<ClientInfo> clientsInfo)
    {
      _lvOnlineClients.Items.Clear();

      foreach (var clientInfo in clientsInfo)
      {
        var item = new ListViewItem(clientInfo.Name)
        {
          Tag = clientInfo
        };
        _lvOnlineClients.Items.Add(item);
      }
    }

    private void OnClientDoubleClick(IClient client)
    {
      throw new NotImplementedException();
    }
  }
}
