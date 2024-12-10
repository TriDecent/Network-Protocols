using ChattingApplication.Enums;

namespace ChattingApplication.Models;

public record Message(ClientInfo Client, byte[] Content, MessageType Type);