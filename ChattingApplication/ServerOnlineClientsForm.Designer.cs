namespace ChattingApplication
{
  partial class ServerOnlineClientsForm
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      lvOnlineClients = new ListView();
      SuspendLayout();
      // 
      // lvOnlineUsers
      // 
      lvOnlineClients.Location = new Point(12, 12);
      lvOnlineClients.Name = "lvOnlineUsers";
      lvOnlineClients.Size = new Size(389, 308);
      lvOnlineClients.TabIndex = 0;
      lvOnlineClients.UseCompatibleStateImageBehavior = false;
      // 
      // ServerOnlineUsersForm
      // 
      AutoScaleDimensions = new SizeF(7F, 15F);
      AutoScaleMode = AutoScaleMode.Font;
      ClientSize = new Size(413, 332);
      Controls.Add(lvOnlineClients);
      Name = "ServerOnlineUsersForm";
      Text = "Online Users";
      ResumeLayout(false);
    }

    #endregion

    private ListView lvOnlineClients;
  }
}