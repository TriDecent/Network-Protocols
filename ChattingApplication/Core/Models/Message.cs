using ChattingApplication.Common.Enums;

namespace ChattingApplication.Core.Models;

public record Message(
  ClientInfo Sender,
  byte[] Content,
  MessageType Type,
  Target Target,
  MessageRequest Request,
  ClientInfo? Recipient = null);