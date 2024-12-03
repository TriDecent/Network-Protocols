using System.Net;
using System.Net.Sockets;
using TCP.Server;
using TCP.Client;

// Client
// using var tcpClient = new TcpClient("192.168.84.128", 1211);
// var client = new Client(tcpClient);
// _ = client.HandleResponseFromServer();

// while (true)
// {
//   var command = GetStringFromUser();
//   client.SendMessage(command);
// }

// static string GetStringFromUser() => Console.ReadLine()!;

// Server
using var listener = new TcpListener(IPAddress.Any, 1211);
var server = new Server(listener);
server.Start();

await server.HandleMultipleClientConnectionsAsync();
