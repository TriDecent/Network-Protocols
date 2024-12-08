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

        var clientForm = new ClientForm();
        var serverForm = new ServerForm();

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