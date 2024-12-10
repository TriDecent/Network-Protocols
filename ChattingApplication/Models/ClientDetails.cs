using System.Net.Sockets;

namespace ChattingApplication.Models;

public record ClientDetails(string Name, TcpClient Client);