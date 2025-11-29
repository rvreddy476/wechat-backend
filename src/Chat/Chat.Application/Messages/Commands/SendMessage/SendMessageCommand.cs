using Chat.Domain.Entities;
using MediatR;
using Shared.Domain.Common;

namespace Chat.Application.Messages.Commands.SendMessage;

public record SendMessageCommand(
    string ConversationId,
    Guid SenderId,
    string SenderUsername,
    string Content,
    MessageType MessageType = MessageType.Text,
    string? MediaUrl = null,
    string? ReplyToMessageId = null
) : IRequest<Result<Message>>;
