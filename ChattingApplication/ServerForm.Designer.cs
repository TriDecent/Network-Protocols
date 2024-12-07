namespace ChattingApplication
{
  partial class ServerForm
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ServerForm));
      btnDetach = new Button();
      btnStop = new Button();
      lblState = new Label();
      btnStart = new Button();
      rtbDialogArea = new RichTextBox();
      btnAttach = new Button();
      btnSend = new Button();
      lblServer = new Label();
      txtMessage = new TextBox();
      SuspendLayout();
      // 
      // btnDetach
      // 
      btnDetach.BackgroundImage = Properties.Resources.attach_file_off;
      btnDetach.BackgroundImageLayout = ImageLayout.Zoom;
      btnDetach.FlatStyle = FlatStyle.Flat;
      btnDetach.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
      btnDetach.Location = new Point(12, 465);
      btnDetach.Name = "btnDetach";
      btnDetach.Size = new Size(48, 31);
      btnDetach.TabIndex = 18;
      btnDetach.UseVisualStyleBackColor = true;
      btnDetach.Visible = false;
      btnDetach.Click += BtnDetach_Click;
      // 
      // btnStop
      // 
      btnStop.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
      btnStop.Location = new Point(410, 10);
      btnStop.Name = "btnStop";
      btnStop.Size = new Size(100, 28);
      btnStop.TabIndex = 17;
      btnStop.Text = "Stop";
      btnStop.UseVisualStyleBackColor = true;
      btnStop.Visible = false;
      btnStop.Click += BtnStop_Click;
      // 
      // lblState
      // 
      lblState.AutoSize = true;
      lblState.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
      lblState.Location = new Point(12, 501);
      lblState.Name = "lblState";
      lblState.Size = new Size(116, 21);
      lblState.TabIndex = 16;
      lblState.Text = "Status: Stopped";
      // 
      // btnStart
      // 
      btnStart.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
      btnStart.Location = new Point(410, 10);
      btnStart.Name = "btnStart";
      btnStart.Size = new Size(100, 28);
      btnStart.TabIndex = 15;
      btnStart.Text = "Start";
      btnStart.UseVisualStyleBackColor = true;
      btnStart.Click += BtnStart_Click;
      // 
      // rtbDialogArea
      // 
      rtbDialogArea.Font = new Font("Segoe UI", 12F);
      rtbDialogArea.Location = new Point(12, 44);
      rtbDialogArea.Name = "rtbDialogArea";
      rtbDialogArea.ReadOnly = true;
      rtbDialogArea.Size = new Size(498, 416);
      rtbDialogArea.TabIndex = 14;
      rtbDialogArea.Text = "";
      // 
      // btnAttach
      // 
      btnAttach.BackgroundImage = Properties.Resources.attachment_icon;
      btnAttach.BackgroundImageLayout = ImageLayout.Zoom;
      btnAttach.FlatStyle = FlatStyle.Flat;
      btnAttach.Font = new Font("Segoe UI", 12F);
      btnAttach.Location = new Point(12, 466);
      btnAttach.Name = "btnAttach";
      btnAttach.Size = new Size(48, 29);
      btnAttach.TabIndex = 13;
      btnAttach.UseVisualStyleBackColor = true;
      btnAttach.Click += BtnAttach_Click;
      // 
      // btnSend
      // 
      btnSend.Font = new Font("Segoe UI", 12F);
      btnSend.Location = new Point(448, 467);
      btnSend.Name = "btnSend";
      btnSend.Size = new Size(62, 29);
      btnSend.TabIndex = 12;
      btnSend.Text = "Send";
      btnSend.UseVisualStyleBackColor = true;
      btnSend.Click += BtnSend_ClickAsync;
      // 
      // lblServer
      // 
      lblServer.AutoSize = true;
      lblServer.Font = new Font("Segoe UI", 12F);
      lblServer.Location = new Point(12, 10);
      lblServer.Name = "lblServer";
      lblServer.Size = new Size(55, 21);
      lblServer.TabIndex = 11;
      lblServer.Text = "Server";
      // 
      // txtMessage
      // 
      txtMessage.Font = new Font("Segoe UI", 12F);
      txtMessage.Location = new Point(66, 466);
      txtMessage.Name = "txtMessage";
      txtMessage.Size = new Size(378, 29);
      txtMessage.TabIndex = 10;
      // 
      // ServerForm
      // 
      AcceptButton = btnSend;
      AutoScaleDimensions = new SizeF(7F, 15F);
      AutoScaleMode = AutoScaleMode.Font;
      ClientSize = new Size(522, 527);
      Controls.Add(btnDetach);
      Controls.Add(btnStop);
      Controls.Add(lblState);
      Controls.Add(btnStart);
      Controls.Add(rtbDialogArea);
      Controls.Add(btnAttach);
      Controls.Add(btnSend);
      Controls.Add(lblServer);
      Controls.Add(txtMessage);
      Icon = (Icon)resources.GetObject("$this.Icon");
      Name = "ServerForm";
      Text = "Server";
      ResumeLayout(false);
      PerformLayout();
    }

    #endregion

    private Button btnDetach;
    private Button btnStop;
    private Label lblState;
    private Button btnStart;
    private RichTextBox rtbDialogArea;
    private Button btnAttach;
    private Button btnSend;
    private Label lblServer;
    private TextBox txtMessage;
  }
}