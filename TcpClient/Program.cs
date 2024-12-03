using System.Net.Sockets;
using TCP.Client;

using var tcpClient = new TcpClient("192.168.2.215", 1211);
var client = new Client(tcpClient);
_ = client.HandleResponseFromServer();

while (true)
{
  var command = GetStringFromUser();
  client.SendMessage(command!);
}

static string? GetStringFromUser() => Console.ReadLine();