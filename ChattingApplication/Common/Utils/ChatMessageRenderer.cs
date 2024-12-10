namespace ChattingApplication.Common.Utils;

public class ChatMessageRenderer(RichTextBox messageDisplay)
{
  private readonly RichTextBox _messageDisplay = messageDisplay;

  public void DisplayMessage(string sender, string message, bool isOwnMessage = false)
  {
    // Create a new paragraph
    _messageDisplay.SelectionStart = _messageDisplay.TextLength;
    _messageDisplay.SelectionLength = 0;

    // Set alignment and padding
    _messageDisplay.SelectionAlignment = isOwnMessage ?
        HorizontalAlignment.Right : HorizontalAlignment.Left;

    // Add padding
    _messageDisplay.SelectionIndent = 10; // Left padding
    _messageDisplay.SelectionRightIndent = 10; // Right padding

    // Add timestamp and sender with appropriate colors
    _messageDisplay.SelectionColor = Color.Gray;
    _messageDisplay.AppendText($"[{DateTime.Now:HH:mm}] ");

    _messageDisplay.SelectionColor = isOwnMessage ?
        Color.Green : Color.Blue;
    _messageDisplay.AppendText($"{sender}: ");

    // Add message with background
    _messageDisplay.SelectionColor = Color.Black;
    _messageDisplay.SelectionBackColor = isOwnMessage ?
        Color.FromArgb(220, 248, 198) : Color.FromArgb(200, 235, 255);
    _messageDisplay.AppendText($"{message}{Environment.NewLine}{Environment.NewLine}");

    // Reset padding and caret
    _messageDisplay.SelectionIndent = 0;
    _messageDisplay.SelectionRightIndent = 0;
    _messageDisplay.SelectionStart = _messageDisplay.TextLength;
    _messageDisplay.ScrollToCaret();
  }

  public void DisplayImage(string sender, Image image, bool isOwnMessage = false)
  {
    // Resize image if needed
    if (image.Width > _messageDisplay.ClientSize.Width - 40)
    {
      float ratio = (float)(_messageDisplay.ClientSize.Width - 40) / image.Width;
      int newWidth = (int)(image.Width * ratio);
      int newHeight = (int)(image.Height * ratio);
      image = new Bitmap(image, new Size(newWidth, newHeight));
    }

    // Start new paragraph
    _messageDisplay.SelectionStart = _messageDisplay.TextLength;
    _messageDisplay.SelectionLength = 0;

    // Add padding
    _messageDisplay.SelectionIndent = isOwnMessage ? 50 : 10; // Padding left for sent/received
    _messageDisplay.SelectionRightIndent = isOwnMessage ? 10 : 50; // Padding right for sent/received

    // Set alignment for the header and image
    _messageDisplay.SelectionAlignment = isOwnMessage ?
        HorizontalAlignment.Right : HorizontalAlignment.Left;

    // Add header
    _messageDisplay.SelectionColor = Color.Gray;
    _messageDisplay.AppendText($"[{DateTime.Now:HH:mm}] ");
    _messageDisplay.SelectionColor = isOwnMessage ? Color.Green : Color.Blue;
    _messageDisplay.AppendText($"{sender}{Environment.NewLine}");

    // Insert image
    _messageDisplay.ReadOnly = false;
    Clipboard.SetImage(image);
    _messageDisplay.Paste();
    _messageDisplay.ReadOnly = true;

    // Add "Sent an image" text
    _messageDisplay.AppendText(Environment.NewLine); // Add spacing
    _messageDisplay.SelectionColor = Color.BlueViolet;
    _messageDisplay.SelectionBackColor = Color.Transparent; // Transparent background
    _messageDisplay.AppendText("Sent an image");
    _messageDisplay.AppendText(Environment.NewLine + Environment.NewLine); // Add additional spacing

    // Reset padding and alignment
    _messageDisplay.SelectionIndent = 0;
    _messageDisplay.SelectionRightIndent = 0;
    _messageDisplay.SelectionAlignment = HorizontalAlignment.Left; // Reset alignment to default
    _messageDisplay.ScrollToCaret();
  }
}
