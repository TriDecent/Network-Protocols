namespace ChattingApplication
{
  partial class ClientForm
  {
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
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
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ClientForm));
      btnMessage = new TextBox();
      lblChatWithServer = new Label();
      btnSend = new Button();
      btnAttach = new Button();
      rtbDialogArea = new RichTextBox();
      btnConnectToServer = new Button();
      lblStatus = new Label();
      SuspendLayout();
      // 
      // btnMessage
      // 
      btnMessage.Font = new Font("Segoe UI", 12F);
      btnMessage.Location = new Point(66, 472);
      btnMessage.Name = "btnMessage";
      btnMessage.Size = new Size(378, 29);
      btnMessage.TabIndex = 0;
      // 
      // lblChatWithServer
      // 
      lblChatWithServer.AutoSize = true;
      lblChatWithServer.Font = new Font("Segoe UI", 12F);
      lblChatWithServer.Location = new Point(14, 19);
      lblChatWithServer.Name = "lblChatWithServer";
      lblChatWithServer.Size = new Size(128, 21);
      lblChatWithServer.TabIndex = 1;
      lblChatWithServer.Text = "Chat With Server";
      // 
      // btnSend
      // 
      btnSend.Font = new Font("Segoe UI", 12F);
      btnSend.Location = new Point(450, 472);
      btnSend.Name = "btnSend";
      btnSend.Size = new Size(62, 29);
      btnSend.TabIndex = 3;
      btnSend.Text = "Send";
      btnSend.UseVisualStyleBackColor = true;
      // 
      // btnAttach
      // 
      btnAttach.BackgroundImage = Properties.Resources.attachment_icon;
      btnAttach.BackgroundImageLayout = ImageLayout.Zoom;
      btnAttach.FlatStyle = FlatStyle.Popup;
      btnAttach.Font = new Font("Segoe UI", 12F);
      btnAttach.Location = new Point(14, 471);
      btnAttach.Name = "btnAttach";
      btnAttach.Size = new Size(48, 29);
      btnAttach.TabIndex = 4;
      btnAttach.UseVisualStyleBackColor = true;
      // 
      // rtbDialogArea
      // 
      rtbDialogArea.Font = new Font("Segoe UI", 12F);
      rtbDialogArea.Location = new Point(14, 49);
      rtbDialogArea.Name = "rtbDialogArea";
      rtbDialogArea.Size = new Size(498, 416);
      rtbDialogArea.TabIndex = 5;
      rtbDialogArea.Text = "";
      // 
      // btnConnectToServer
      // 
      btnConnectToServer.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
      btnConnectToServer.Location = new Point(412, 15);
      btnConnectToServer.Name = "btnConnectToServer";
      btnConnectToServer.Size = new Size(100, 28);
      btnConnectToServer.TabIndex = 6;
      btnConnectToServer.Text = "Connect";
      btnConnectToServer.UseVisualStyleBackColor = true;
      // 
      // lblStatus
      // 
      lblStatus.AutoSize = true;
      lblStatus.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
      lblStatus.Location = new Point(14, 506);
      lblStatus.Name = "lblStatus";
      lblStatus.Size = new Size(152, 21);
      lblStatus.TabIndex = 7;
      lblStatus.Text = "Status: Disconnected";
      // 
      // ClientForm
      // 
      AutoScaleDimensions = new SizeF(7F, 15F);
      AutoScaleMode = AutoScaleMode.Font;
      ClientSize = new Size(535, 536);
      Controls.Add(lblStatus);
      Controls.Add(btnConnectToServer);
      Controls.Add(rtbDialogArea);
      Controls.Add(btnAttach);
      Controls.Add(btnSend);
      Controls.Add(lblChatWithServer);
      Controls.Add(btnMessage);
      Icon = (Icon)resources.GetObject("$this.Icon");
      Name = "ClientForm";
      Text = "Client";
      ResumeLayout(false);
      PerformLayout();
    }

    #endregion

    private TextBox btnMessage;
    private Label lblChatWithServer;
    private Button btnSend;
    private Button btnAttach;
    private RichTextBox rtbDialogArea;
    private Button btnConnectToServer;
    private Label lblStatus;
  }
}
