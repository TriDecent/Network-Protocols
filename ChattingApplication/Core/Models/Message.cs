using ChattingApplication.Common.Enums;

namespace ChattingApplication.Core.Models;

public record Message(ClientInfo Client, byte[] Content, MessageType Type);