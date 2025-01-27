namespace ChattingApplication
{
  partial class ClientOnlineClientsForm
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
      components = new System.ComponentModel.Container();
      lvOnlineClients = new ListView();
      timerUpdateClientsInfo = new System.Windows.Forms.Timer(components);
      SuspendLayout();
      // 
      // lvOnlineClients
      // 
      lvOnlineClients.Location = new Point(12, 12);
      lvOnlineClients.Name = "lvOnlineClients";
      lvOnlineClients.Size = new Size(389, 308);
      lvOnlineClients.TabIndex = 1;
      lvOnlineClients.UseCompatibleStateImageBehavior = false;
      // 
      // timerUpdateClientsInfo
      // 
      timerUpdateClientsInfo.Interval = 1500;
      // 
      // ClientOnlineClientsForm
      // 
      AutoScaleDimensions = new SizeF(7F, 15F);
      AutoScaleMode = AutoScaleMode.Font;
      ClientSize = new Size(413, 333);
      Controls.Add(lvOnlineClients);
      Name = "ClientOnlineClientsForm";
      Text = "Online Users";
      ResumeLayout(false);
    }

    #endregion

    private ListView lvOnlineClients;
    private System.Windows.Forms.Timer timerUpdateClientsInfo;
  }
}