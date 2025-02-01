namespace ChattingApplication
{
  partial class ServerDirectMessageForm
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
      btnDetach = new Button();
      rtbDialogArea = new RichTextBox();
      btnAttach = new Button();
      btnSend = new Button();
      lblDM = new Label();
      txtMessage = new TextBox();
      lblClientName = new Label();
      RecipientActivityCheckTimer = new System.Windows.Forms.Timer(components);
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
      btnDetach.TabIndex = 32;
      btnDetach.UseVisualStyleBackColor = true;
      btnDetach.Visible = false;
      // 
      // rtbDialogArea
      // 
      rtbDialogArea.Font = new Font("Segoe UI", 12F);
      rtbDialogArea.Location = new Point(12, 44);
      rtbDialogArea.Name = "rtbDialogArea";
      rtbDialogArea.ReadOnly = true;
      rtbDialogArea.Size = new Size(498, 416);
      rtbDialogArea.TabIndex = 28;
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
      btnAttach.TabIndex = 27;
      btnAttach.UseVisualStyleBackColor = true;
      // 
      // btnSend
      // 
      btnSend.Font = new Font("Segoe UI", 12F);
      btnSend.Location = new Point(448, 465);
      btnSend.Name = "btnSend";
      btnSend.Size = new Size(62, 31);
      btnSend.TabIndex = 26;
      btnSend.Text = "Send";
      btnSend.UseVisualStyleBackColor = true;
      // 
      // lblDM
      // 
      lblDM.AutoSize = true;
      lblDM.Font = new Font("Segoe UI", 12F);
      lblDM.Location = new Point(12, 10);
      lblDM.Name = "lblDM";
      lblDM.Size = new Size(134, 21);
      lblDM.TabIndex = 25;
      lblDM.Text = "Direct message to";
      // 
      // txtMessage
      // 
      txtMessage.Font = new Font("Segoe UI", 12F);
      txtMessage.Location = new Point(66, 466);
      txtMessage.Name = "txtMessage";
      txtMessage.Size = new Size(378, 29);
      txtMessage.TabIndex = 24;
      // 
      // lblClientName
      // 
      lblClientName.AutoSize = true;
      lblClientName.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold | FontStyle.Italic | FontStyle.Underline, GraphicsUnit.Point, 0);
      lblClientName.Location = new Point(152, 10);
      lblClientName.Name = "lblClientName";
      lblClientName.Size = new Size(84, 21);
      lblClientName.TabIndex = 33;
      lblClientName.Text = "Trí Decent";
      // 
      // CheckingIfPartnerOnlineTimer
      // 
      RecipientActivityCheckTimer.Interval = 1000;
      // 
      // ServerDirectMessageForm
      // 
      AutoScaleDimensions = new SizeF(7F, 15F);
      AutoScaleMode = AutoScaleMode.Font;
      ClientSize = new Size(524, 513);
      Controls.Add(lblClientName);
      Controls.Add(btnDetach);
      Controls.Add(rtbDialogArea);
      Controls.Add(btnAttach);
      Controls.Add(btnSend);
      Controls.Add(lblDM);
      Controls.Add(txtMessage);
      FormBorderStyle = FormBorderStyle.Fixed3D;
      Name = "ServerDirectMessageForm";
      Text = "DMs";
      ResumeLayout(false);
      PerformLayout();
    }

    #endregion

    private Label lblConnectedClients;
    private Label lblConnected;
    private Button btnDetach;
    private Label lblState;
    private RichTextBox rtbDialogArea;
    private Button btnAttach;
    private Button btnSend;
    private Label lblDM;
    private TextBox txtMessage;
    private Label lblClientName;
    private System.Windows.Forms.Timer RecipientActivityCheckTimer;
  }
}