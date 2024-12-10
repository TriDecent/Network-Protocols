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
      txtMessage = new TextBox();
      lblServerIP = new Label();
      btnSend = new Button();
      btnAttach = new Button();
      rtbDialogArea = new RichTextBox();
      btnConnectServer = new Button();
      lblState = new Label();
      btnDisconnectServer = new Button();
      btnDetach = new Button();
      txtServerIP = new TextBox();
      txtServerPort = new TextBox();
      lblServerPort = new Label();
      SuspendLayout();
      // 
      // txtMessage
      // 
      txtMessage.Font = new Font("Segoe UI", 12F);
      txtMessage.Location = new Point(68, 471);
      txtMessage.Name = "txtMessage";
      txtMessage.Size = new Size(378, 29);
      txtMessage.TabIndex = 0;
      // 
      // lblServerIP
      // 
      lblServerIP.AutoSize = true;
      lblServerIP.Font = new Font("Segoe UI", 12F);
      lblServerIP.Location = new Point(14, 19);
      lblServerIP.Name = "lblServerIP";
      lblServerIP.Size = new Size(23, 21);
      lblServerIP.TabIndex = 1;
      lblServerIP.Text = "IP";
      // 
      // btnSend
      // 
      btnSend.Font = new Font("Segoe UI", 12F);
      btnSend.Location = new Point(450, 471);
      btnSend.Name = "btnSend";
      btnSend.Size = new Size(62, 29);
      btnSend.TabIndex = 3;
      btnSend.Text = "Send";
      btnSend.UseVisualStyleBackColor = true;
      btnSend.Click += BtnSend_Click;
      // 
      // btnAttach
      // 
      btnAttach.BackgroundImage = Properties.Resources.attachment_icon;
      btnAttach.BackgroundImageLayout = ImageLayout.Zoom;
      btnAttach.FlatStyle = FlatStyle.Flat;
      btnAttach.Font = new Font("Segoe UI", 12F);
      btnAttach.Location = new Point(14, 471);
      btnAttach.Name = "btnAttach";
      btnAttach.Size = new Size(48, 29);
      btnAttach.TabIndex = 4;
      btnAttach.UseVisualStyleBackColor = true;
      btnAttach.Click += BtnAttach_Click;
      // 
      // rtbDialogArea
      // 
      rtbDialogArea.Font = new Font("Segoe UI", 12F);
      rtbDialogArea.Location = new Point(14, 49);
      rtbDialogArea.Name = "rtbDialogArea";
      rtbDialogArea.ReadOnly = true;
      rtbDialogArea.Size = new Size(498, 416);
      rtbDialogArea.TabIndex = 5;
      rtbDialogArea.Text = "";
      // 
      // btnConnectServer
      // 
      btnConnectServer.Enabled = false;
      btnConnectServer.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
      btnConnectServer.Location = new Point(412, 15);
      btnConnectServer.Name = "btnConnectServer";
      btnConnectServer.Size = new Size(100, 28);
      btnConnectServer.TabIndex = 6;
      btnConnectServer.Text = "Connect";
      btnConnectServer.UseVisualStyleBackColor = true;
      btnConnectServer.Click += BtnConnectToServer_Click;
      // 
      // lblState
      // 
      lblState.AutoSize = true;
      lblState.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
      lblState.Location = new Point(14, 506);
      lblState.Name = "lblState";
      lblState.Size = new Size(152, 21);
      lblState.TabIndex = 7;
      lblState.Text = "Status: Disconnected";
      // 
      // btnDisconnectServer
      // 
      btnDisconnectServer.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
      btnDisconnectServer.Location = new Point(412, 14);
      btnDisconnectServer.Name = "btnDisconnectServer";
      btnDisconnectServer.Size = new Size(100, 28);
      btnDisconnectServer.TabIndex = 8;
      btnDisconnectServer.Text = "Disconnect";
      btnDisconnectServer.UseVisualStyleBackColor = true;
      btnDisconnectServer.Visible = false;
      btnDisconnectServer.Click += BtnDisconnectServer_Click;
      // 
      // btnDetach
      // 
      btnDetach.BackgroundImage = Properties.Resources.attach_file_off;
      btnDetach.BackgroundImageLayout = ImageLayout.Zoom;
      btnDetach.FlatStyle = FlatStyle.Flat;
      btnDetach.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
      btnDetach.Location = new Point(14, 471);
      btnDetach.Name = "btnDetach";
      btnDetach.Size = new Size(48, 29);
      btnDetach.TabIndex = 9;
      btnDetach.UseVisualStyleBackColor = true;
      btnDetach.Visible = false;
      btnDetach.Click += BtnDetach_Click;
      // 
      // txtServerIP
      // 
      txtServerIP.Font = new Font("Segoe UI", 12F);
      txtServerIP.Location = new Point(43, 14);
      txtServerIP.Name = "txtServerIP";
      txtServerIP.Size = new Size(123, 29);
      txtServerIP.TabIndex = 10;
      // 
      // txtServerPort
      // 
      txtServerPort.Font = new Font("Segoe UI", 12F);
      txtServerPort.Location = new Point(216, 14);
      txtServerPort.Name = "txtServerPort";
      txtServerPort.Size = new Size(100, 29);
      txtServerPort.TabIndex = 11;
      // 
      // lblServerPort
      // 
      lblServerPort.AutoSize = true;
      lblServerPort.Font = new Font("Segoe UI", 12F);
      lblServerPort.Location = new Point(172, 19);
      lblServerPort.Name = "lblServerPort";
      lblServerPort.Size = new Size(38, 21);
      lblServerPort.TabIndex = 1;
      lblServerPort.Text = "Port";
      // 
      // ClientForm
      // 
      AcceptButton = btnSend;
      AutoScaleDimensions = new SizeF(7F, 15F);
      AutoScaleMode = AutoScaleMode.Font;
      ClientSize = new Size(528, 535);
      Controls.Add(txtServerPort);
      Controls.Add(txtServerIP);
      Controls.Add(btnDetach);
      Controls.Add(btnDisconnectServer);
      Controls.Add(lblState);
      Controls.Add(btnConnectServer);
      Controls.Add(rtbDialogArea);
      Controls.Add(btnAttach);
      Controls.Add(btnSend);
      Controls.Add(lblServerPort);
      Controls.Add(lblServerIP);
      Controls.Add(txtMessage);
      FormBorderStyle = FormBorderStyle.FixedDialog;
      Icon = (Icon)resources.GetObject("$this.Icon");
      MaximizeBox = false;
      Name = "ClientForm";
      Text = "Client";
      ResumeLayout(false);
      PerformLayout();
    }

    #endregion

    private TextBox txtMessage;
    private Label lblServerIP;
    private Button btnSend;
    private Button btnAttach;
    private RichTextBox rtbDialogArea;
    private Button btnConnectServer;
    private Label lblState;
    private Button btnDisconnectServer;
    private Button btnDetach;
    private TextBox txtServerIP;
    private TextBox txtServerPort;
    private Label lblServerPort;
  }
}
