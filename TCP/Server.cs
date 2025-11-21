using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TCP.Server;

public class Server(TcpListener listener)
{
  private readonly TcpListener _listener = listener;
  private readonly List<TcpClient> _clients = [];

  public void Start() => _listener.Start();

  public void Stop() => _listener.Stop();

  public async Task HandleMultipleClientConnectionsAsync()
  {
    _ = Task.Run(BroadCastMessage);

    await HandleClients();
  }

  private void BroadCastMessage()
  {
    while (true)
    {
      var message = GetStringFromServer();

      lock (_clients)
      {
        foreach (var client in _clients)
        {
          _ = SendMessageToClient(client, message);
        }
      }
    }
  }

  private async Task HandleClients()
  {
    while (true)
    {
      var client = await _listener.AcceptTcpClientAsync();

      lock (_clients)
      {
        _clients.Add(client);
      }

      _ = HandleClientAsync(client);
    }
  }

  private async Task HandleClientAsync(TcpClient client)
  {
    await ProcessIncomingDataFromAsync(client);

    lock (_clients)
    {
      _clients.Remove(client);
    }
  }

  private static async Task ProcessIncomingDataFromAsync(TcpClient client)
  {
    using var stream = client.GetStream();

    var buffer = new byte[1024];
    int bytesReadCount;

    while ((bytesReadCount = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length))) != 0)
    {
      var receivedData = Encoding.UTF8.GetString(buffer, 0, bytesReadCount).TrimEnd('\r', '\n');
      Console.WriteLine($"Received Data: {receivedData}");
    }
  }

  private static async Task SendMessageToClient(TcpClient client, string message)
  {
    var bytes = Encoding.UTF8.GetBytes(message + Environment.NewLine);
    var stream = client.GetStream();
    await stream.WriteAsync(bytes);
  }

  private static string GetStringFromServer() => Console.ReadLine()!;
}