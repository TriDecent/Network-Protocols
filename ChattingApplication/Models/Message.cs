using ChattingApplication.Enums;

namespace ChattingApplication.Models;

public record Message(ClientDetails Client, object Content, MessageType Type);