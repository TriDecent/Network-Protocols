using System.Net;
using System.Net.Sockets;
using TCP.Server;
using TCP.Client;

// Client
// using var tcpClient = new TcpClient("192.168.84.128", 1211);
// var client = new Client(tcpClient);

// while (true)
// {
//   var command = GetStringFromUser();
//   client.SendMessage(command);
// }

// Server
using var listener = new TcpListener(IPAddress.Any, 1211);
var server = new Server(listener);
server.Start();

_ = server.HandleMultipleClientConnectionsAsync();

while (true)
{
  GetStringFromUser();
}

static string GetStringFromUser() => Console.ReadLine()!;