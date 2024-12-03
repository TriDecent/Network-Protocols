using System.Net;
using System.Net.Sockets;
using UDP.Server;

// Client
// using var udpClient = new UdpClient("192.168.84.128", 1211);
// var client = new Client(udpClient);

// while (true)
// {
//   var message = GetStringFromUser();
//   client.Send(message);
// }

// Server

using var udpListener = new UdpClient(1211);

var server = new Server(udpListener);
_ = server.HandleMultipleResponses();

while (true)
{
  GetStringFromUser();
}

static string? GetStringFromUser() => Console.ReadLine();