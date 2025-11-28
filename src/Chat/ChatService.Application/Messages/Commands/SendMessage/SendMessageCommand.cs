using MediatR;
using ChatService.Domain.Entities;
using Shared.Domain.Common;

namespace ChatService.Application.Messages.Commands.SendMessage;

public record SendMessageCommand : IRequest<Result<Message>>
{
    public required string ConversationId { get; init; }
    public required Guid SenderId { get; init; }
    public required string SenderUsername { get; init; }
    public required string Content { get; init; }
    public MessageType MessageType { get; init; } = MessageType.Text;
    public string? MediaUrl { get; init; }
    public string? MediaThumbnailUrl { get; init; }
    public int? MediaDuration { get; init; }
    public string? FileName { get; init; }
    public long? FileSize { get; init; }
    public MessageLocation? Location { get; init; }
    public string? ReplyToMessageId { get; init; }
    public List<Guid>? Mentions { get; init; }
}
