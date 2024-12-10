namespace ChattingApplication.Common.Utils;

public static class ImageByteConverter
{
  public static bool IsImage(this byte[] bytes)
  {
    if (bytes.Length < 4) return false;

    return bytes[0] == 0xFF && bytes[1] == 0xD8 // JPEG
        || bytes.AsSpan(0, 4).SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47 }) // PNG
        || bytes.AsSpan(0, 2).SequenceEqual("BM"u8); // BMP
  }

  public static byte[] ImageToBytes(Image image)
  {
    using var memoryStream = new MemoryStream();
    image.Save(memoryStream, image.RawFormat);

    return memoryStream.ToArray();
  }

  public static Image BytesToImage(this byte[] bytes)
  {
    using var memoryStream = new MemoryStream(bytes);
    var image = Image.FromStream(memoryStream);

    return image;
  }
}
