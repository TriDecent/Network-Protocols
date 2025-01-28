using System.Net.Sockets;

namespace ChattingApplication.Core.Models;

public record ClientSessionInfo(string Id, ClientInfo Info, TcpClient Client);
