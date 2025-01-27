using ChattingApplication.Core.Interfaces;

namespace ChattingApplication
{
  public partial class DirectMessageForm : Form
  {
    public DirectMessageForm(IServer server)
    {
      InitializeComponent();
    }
  }
}
