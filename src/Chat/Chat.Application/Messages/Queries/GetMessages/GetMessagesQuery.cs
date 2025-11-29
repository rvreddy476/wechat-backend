using Chat.Domain.Entities;
using MediatR;
using Shared.Domain.Common;

namespace Chat.Application.Messages.Queries.GetMessages;

public record GetMessagesQuery(string ConversationId, int Page = 1, int PageSize = 50) : IRequest<Result<List<Message>>>;
