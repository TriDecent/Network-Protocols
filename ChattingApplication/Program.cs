using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using ChattingApplication.Common.Utils;
using ChattingApplication.Core.Models;
using ChattingApplication.Core.Serializers;
using ChattingApplication.Core.Sockets;
using ChattingApplication.Infrastructure.Network.Client;
using ChattingApplication.Infrastructure.Network.Client.Connection;
using ChattingApplication.Infrastructure.Network.Client.EventEmitter;
using ChattingApplication.Infrastructure.Network.Client.MessageProcessor;
using ChattingApplication.Infrastructure.Network.Server;
using ChattingApplication.Infrastructure.Network.Server.Connection;
using ChattingApplication.Infrastructure.Network.Server.EventEmitter;
using ChattingApplication.Infrastructure.Network.Server.MessageProcessor;

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

      using var connection1 = new TcpClientConnection(new WrapperTcpClient(tcpClient1));
      using var connection2 = new TcpClientConnection(new WrapperTcpClient(tcpClient2));
      using var connection3 = new TcpClientConnection(new WrapperTcpClient(tcpClient3));

      var serverName = "Trí-Decent-Server";

      var clientAuthOptions = new SslClientAuthenticationOptions
      {
        TargetHost = serverName,
        EnabledSslProtocols = SslProtocols.Tls13 | SslProtocols.Tls12
      };

      using var sslConnection1 = new SslClientConnectionDecorator(
        connection1, clientAuthOptions);
      using var sslConnection2 = new SslClientConnectionDecorator(
        connection2, clientAuthOptions);
      using var sslConnection3 = new SslClientConnectionDecorator(
        connection3, clientAuthOptions);

      // not use using, let form handle life cycle
      var account1 = new ClientInfo("", "");
      var account2 = new ClientInfo("", "");
      var account3 = new ClientInfo("", "");

      var clientMessageProcessor1 = new ClientSideMessageProcessor(serializer, clientEventEmitter1);
      var clientMessageProcessor2 = new ClientSideMessageProcessor(serializer, clientEventEmitter2);
      var clientMessageProcessor3 = new ClientSideMessageProcessor(serializer, clientEventEmitter3);

      var client1 = new Client(sslConnection1, account1, clientEventEmitter1, clientMessageProcessor1);
      var client2 = new Client(sslConnection2, account2, clientEventEmitter2, clientMessageProcessor2);
      var client3 = new Client(sslConnection3, account3, clientEventEmitter3, clientMessageProcessor3);
      // not use using, let form handle life cycle

      var clientForm1 = new ClientForm(client1, clientEventEmitter1);
      var clientForm2 = new ClientForm(client2, clientEventEmitter2);
      var clientForm3 = new ClientForm(client3, clientEventEmitter3);

      var ipEndPoint = new IPEndPoint(IPAddress.Parse("192.168.31.191"), 1211);
      using var tcpListener = new WrapperTcpListener(new TcpListener(ipEndPoint));
      using var tcpServerConnection = new TcpServerConnection(tcpListener);
      using var sslServerConnection = new SslServerConnectionDecorator(
        tcpServerConnection,
        new SslServerAuthenticationOptions
        {
          ServerCertificate = CertificateHelper.GenerateSelfSignedCertificate(
            serverName,
            "very-secure-password-but-hardcoded-and-i-know-that"),
          ClientCertificateRequired = false,
          EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13
        });

      var serverMessageProcessor = new ServerSideMessageProcessor(
        serializer,
        serverEventEmitter);

      var server = new Server(  // not use using, let form handle life cycle
        sslServerConnection,
        serverEventEmitter,
        serverMessageProcessor);

      serverMessageProcessor.RegisterServerOperations(server);

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