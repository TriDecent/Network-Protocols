using System.Net.Sockets;
using System.Text;

namespace UDP.Server
{
  public class Server(UdpClient listener)
  {
    private readonly UdpClient _listener = listener;

    public async Task HandleMultipleResponses()
    {
      while (true)
      {
        var response = await _listener.ReceiveAsync();

        HandleResponse(response);
      }
    }

    private static void HandleResponse(UdpReceiveResult response)
    {
      var buffer = response.Buffer;
      var receivedData = Encoding.UTF8.GetString(buffer).TrimEnd('\r', '\n');

      Console.WriteLine($"Received Data: {receivedData}");
    }
  }
}