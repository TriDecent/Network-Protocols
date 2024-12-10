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

      using var tcpClient = new TcpClient();
      var client = new Client.Client(tcpClient);  // not use using, let form handle life cycle

      var clientForm = new ClientForm(client);

      var ipEndPoint = new IPEndPoint(IPAddress.Parse("192.168.2.215"), 1211);
      using var tcpListener = new TcpListener(ipEndPoint);
      var server = new Server.Server(tcpListener);  // not use using, let form handle life cycle

      var serverForm = new ServerForm(server);

      var hiddenMainForm = new Form()
      {
        Opacity = 0,
        ShowInTaskbar = false
      };

      hiddenMainForm.Load += (s, e) =>
      {
        serverForm.Show();
        clientForm.Show();
        hiddenMainForm.Hide();
      };

      serverForm.FormClosed += CloseHiddenFormIfNoOpenForms;
      clientForm.FormClosed += CloseHiddenFormIfNoOpenForms;

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