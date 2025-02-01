using ChattingApplication.Core.Models;
using ChattingApplication.Core.Serializers;
using ChattingApplication.Infrastructure.Network.Client;
using ChattingApplication.Infrastructure.Network.Client.Connection;
using ChattingApplication.Infrastructure.Network.Client.EventEmitter;
using ChattingApplication.Infrastructure.Network.Server;
using ChattingApplication.Infrastructure.Network.Server.Connection;
using ChattingApplication.Infrastructure.Network.Server.EventEmitter;
using System.Net;
using System.Net.Sockets;

namespace ChattingApplication
{
  internal static class Program
  {
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
      // To customize application configuration such as set high DPI settings or default font,
      // see https://aka.ms/applicationconfiguration.
      ApplicationConfiguration.Initialize();
      // Application.Run(new ServerForm());

      var serializer = new MessageSerializer();

      var clientEventEmitter1 = new ClientEventEmitter();
      var clientEventEmitter2 = new ClientEventEmitter();
      var clientEventEmitter3 = new ClientEventEmitter();

      var serverEventEmitter = new ServerEventEmitter();

      using var tcpClient1 = new TcpClient();
      using var tcpClient2 = new TcpClient();
      using var tcpClient3 = new TcpClient();

      var connection1 = new TcpClientConnection(tcpClient1);
      var connection2 = new TcpClientConnection(tcpClient2);
      var connection3 = new TcpClientConnection(tcpClient3);

      // not use using, let form handle life cycle
      var account1 = new ClientInfo("", "");
      var account2 = new ClientInfo("", "");
      var account3 = new ClientInfo("", "");

      var client1 = new Client(connection1, account1, serializer, clientEventEmitter1);
      var client2 = new Client(connection2, account2, serializer, clientEventEmitter2);
      var client3 = new Client(connection3, account3, serializer, clientEventEmitter3);
      // not use using, let form handle life cycle

      var clientForm1 = new ClientForm(client1, clientEventEmitter1);
      var clientForm2 = new ClientForm(client2, clientEventEmitter2);
      var clientForm3 = new ClientForm(client3, clientEventEmitter3);

      var ipEndPoint = new IPEndPoint(IPAddress.Parse("192.168.2.215"), 1211);
      using var tcpListener = new TcpListener(ipEndPoint);
      using var tcpServerConnection = new TcpServerConnection(tcpListener);
      var server = new Server(tcpServerConnection, serializer, serverEventEmitter);  // not use using, let form handle life cycle

      var serverForm = new ServerForm(server, serverEventEmitter);

      var hiddenMainForm = new Form()
      {
        Opacity = 0,
        ShowInTaskbar = false
      };

      hiddenMainForm.Load += (s, e) =>
      {
        serverForm.Show();
        clientForm1.Show();
        clientForm2.Show();
        clientForm3.Show();
        hiddenMainForm.Hide();
      };

      serverForm.FormClosed += CloseHiddenFormIfNoOpenForms;
      clientForm1.FormClosed += CloseHiddenFormIfNoOpenForms;
      clientForm2.FormClosed += CloseHiddenFormIfNoOpenForms;
      clientForm3.FormClosed += CloseHiddenFormIfNoOpenForms;

      Application.Run(hiddenMainForm);

      void CloseHiddenFormIfNoOpenForms(object? sender, FormClosedEventArgs e)
      {
        if (Application.OpenForms.Count == 1)
        {
          hiddenMainForm.Close();
        }
      }
    }
  }
}