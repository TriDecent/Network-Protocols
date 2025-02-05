using ChattingApplication.Core.Interfaces;

namespace ChattingApplication.Core.Models;

public record ClientSessionInfo(ClientInfo Info, ITcpClient Client);
