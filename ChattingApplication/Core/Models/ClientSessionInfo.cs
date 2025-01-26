using System.Net.Sockets;

namespace ChattingApplication.Core.Models;

public record ClientSessionInfo(ClientInfo Info, TcpClient Client);
