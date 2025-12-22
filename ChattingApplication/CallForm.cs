using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace ChattingApplication;

public partial class CallForm : Form
{
  private readonly string _callUrl;
  private WebView2? _webView2;

  public CallForm(string callUrl, Action onCallEnded)
  {
    InitializeComponent();

    _callUrl = callUrl;

    FormClosing += (s, e) =>
    {
      if (_webView2 is null) return;
      _webView2.Dispose();
      onCallEnded();
    };
  }

  private async void CallForm_Load(object sender, EventArgs e)
  {
    _webView2 = webView21;

    await _webView2.EnsureCoreWebView2Async();
    _webView2.CoreWebView2.PermissionRequested += (sender, args) =>
    {
      if (args.Uri.Contains(_callUrl) || args.Uri.Contains("localhost"))
      {
        args.State = args.PermissionKind switch
        {
          CoreWebView2PermissionKind.Microphone or
          CoreWebView2PermissionKind.Camera => CoreWebView2PermissionState.Allow,
          _ => CoreWebView2PermissionState.Default,
        };
      }
    };

    _webView2.CoreWebView2.WebMessageReceived += (sender, args) =>
    {
      try
      {

        var jsonString = args.TryGetWebMessageAsString();
        if (!jsonString.Contains("END_CALL")) return;
        Close();
      }
      catch
      {

      }
    };

    _webView2.Source = new Uri(_callUrl);
  }
}
