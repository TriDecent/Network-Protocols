using ChattingApplication.Core.Models;
using ChattingApplication.Core.Serializers;
using ChattingApplication.Infrastructure.Network.Client;
using ChattingApplication.Infrastructure.Network.Server;
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

      var eventEmitter1 = new ClientEventEmitter();
      var eventEmitter2 = new ClientEventEmitter();
      var eventEmitter3 = new ClientEventEmitter();

      using var tcpClient1 = new TcpClient();
      using var tcpClient2 = new TcpClient();
      using var tcpClient3 = new TcpClient();

      var account1 = new ClientInfo("", "");
      var account2 = new ClientInfo("", "");
      var account3 = new ClientInfo("", "");

      var client1 = new Client(tcpClient1, account1, serializer, eventEmitter1);  // not use using, let form handle life cycle
      var client2 = new Client(tcpClient2, account2, serializer, eventEmitter2);  // not use using, let form handle life cycle
      var client3 = new Client(tcpClient3, account3, serializer, eventEmitter3);  // not use using, let form handle life cycle

      var clientForm1 = new ClientForm(client1, eventEmitter1);
      var clientForm2 = new ClientForm(client2, eventEmitter2);
      var clientForm3 = new ClientForm(client3, eventEmitter3);

      var ipEndPoint = new IPEndPoint(IPAddress.Parse("192.168.2.215"), 1211);
      using var tcpListener = new TcpListener(ipEndPoint);
      var server = new Server(tcpListener, serializer);  // not use using, let form handle life cycle

      var serverForm = new ServerForm(server);

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