using System.Net;

namespace ChattingApplication.Utils;

internal static partial class IPAddressValidator
{
  public static bool IsValidIP(string ip) => IPAddress.TryParse(ip, out _);

  public static bool IsValidPort(string port)
  {
    const int MinPort = 0;
    const int MaxPort = 65535;

    return !string.IsNullOrWhiteSpace(port) &&
      int.TryParse(port, out var portNumber) &&
      portNumber >= MinPort &&
      portNumber <= MaxPort;
  }
}