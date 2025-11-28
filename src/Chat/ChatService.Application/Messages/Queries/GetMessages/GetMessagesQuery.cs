using MediatR;
using ChatService.Domain.Entities;
using Shared.Domain.Common;

namespace ChatService.Application.Messages.Queries.GetMessages;

public record GetMessagesQuery : IRequest<Result<List<Message>>>
{
    public required string ConversationId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}
