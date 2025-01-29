namespace ChattingApplication
{
  partial class ClientDirectMessageForm
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
      lblRecipientName = new Label();
      btnDetach = new Button();
      rtbDialogArea = new RichTextBox();
      btnAttach = new Button();
      btnSend = new Button();
      lblDM = new Label();
      txtMessage = new TextBox();
      lblSenderName = new Label();
      SuspendLayout();
      // 
      // lblRecipientName
      // 
      lblRecipientName.AutoSize = true;
      lblRecipientName.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold | FontStyle.Italic | FontStyle.Underline, GraphicsUnit.Point, 0);
      lblRecipientName.Location = new Point(152, 14);
      lblRecipientName.Name = "lblRecipientName";
      lblRecipientName.Size = new Size(67, 21);
      lblRecipientName.TabIndex = 40;
      lblRecipientName.Text = "Trí Trần";
      // 
      // btnDetach
      // 
      btnDetach.BackgroundImage = Properties.Resources.attach_file_off;
      btnDetach.BackgroundImageLayout = ImageLayout.Zoom;
      btnDetach.FlatStyle = FlatStyle.Flat;
      btnDetach.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
      btnDetach.Location = new Point(12, 469);
      btnDetach.Name = "btnDetach";
      btnDetach.Size = new Size(48, 31);
      btnDetach.TabIndex = 39;
      btnDetach.UseVisualStyleBackColor = true;
      btnDetach.Visible = false;
      // 
      // rtbDialogArea
      // 
      rtbDialogArea.Font = new Font("Segoe UI", 12F);
      rtbDialogArea.Location = new Point(12, 48);
      rtbDialogArea.Name = "rtbDialogArea";
      rtbDialogArea.ReadOnly = true;
      rtbDialogArea.Size = new Size(498, 416);
      rtbDialogArea.TabIndex = 38;
      rtbDialogArea.Text = "";
      // 
      // btnAttach
      // 
      btnAttach.BackgroundImage = Properties.Resources.attachment_icon;
      btnAttach.BackgroundImageLayout = ImageLayout.Zoom;
      btnAttach.FlatStyle = FlatStyle.Flat;
      btnAttach.Font = new Font("Segoe UI", 12F);
      btnAttach.Location = new Point(12, 470);
      btnAttach.Name = "btnAttach";
      btnAttach.Size = new Size(48, 29);
      btnAttach.TabIndex = 37;
      btnAttach.UseVisualStyleBackColor = true;
      // 
      // btnSend
      // 
      btnSend.Font = new Font("Segoe UI", 12F);
      btnSend.Location = new Point(448, 469);
      btnSend.Name = "btnSend";
      btnSend.Size = new Size(62, 31);
      btnSend.TabIndex = 36;
      btnSend.Text = "Send";
      btnSend.UseVisualStyleBackColor = true;
      // 
      // lblDM
      // 
      lblDM.AutoSize = true;
      lblDM.Font = new Font("Segoe UI", 12F);
      lblDM.Location = new Point(12, 14);
      lblDM.Name = "lblDM";
      lblDM.Size = new Size(134, 21);
      lblDM.TabIndex = 35;
      lblDM.Text = "Direct message to";
      // 
      // txtMessage
      // 
      txtMessage.Font = new Font("Segoe UI", 12F);
      txtMessage.Location = new Point(66, 470);
      txtMessage.Name = "txtMessage";
      txtMessage.Size = new Size(378, 29);
      txtMessage.TabIndex = 34;
      // 
      // lblSenderName
      // 
      lblSenderName.AutoSize = true;
      lblSenderName.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold | FontStyle.Italic | FontStyle.Underline, GraphicsUnit.Point, 0);
      lblSenderName.Location = new Point(426, 14);
      lblSenderName.Name = "lblSenderName";
      lblSenderName.Size = new Size(84, 21);
      lblSenderName.TabIndex = 41;
      lblSenderName.Text = "Trí Decent";
      // 
      // ClientDirectMessageForm
      // 
      AutoScaleDimensions = new SizeF(7F, 15F);
      AutoScaleMode = AutoScaleMode.Font;
      ClientSize = new Size(526, 514);
      Controls.Add(lblSenderName);
      Controls.Add(lblRecipientName);
      Controls.Add(btnDetach);
      Controls.Add(rtbDialogArea);
      Controls.Add(btnAttach);
      Controls.Add(btnSend);
      Controls.Add(lblDM);
      Controls.Add(txtMessage);
      FormBorderStyle = FormBorderStyle.Fixed3D;
      Name = "ClientDirectMessageForm";
      Text = "DMs";
      ResumeLayout(false);
      PerformLayout();
    }

    #endregion

    private Label lblRecipientName;
    private Button btnDetach;
    private RichTextBox rtbDialogArea;
    private Button btnAttach;
    private Button btnSend;
    private Label lblDM;
    private TextBox txtMessage;
    private Label lblSenderName;
  }
}