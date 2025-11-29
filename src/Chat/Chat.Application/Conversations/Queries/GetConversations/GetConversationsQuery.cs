using Chat.Domain.Entities;
using MediatR;
using Shared.Domain.Common;

namespace Chat.Application.Conversations.Queries.GetConversations;

public record GetConversationsQuery(Guid UserId, int Page = 1, int PageSize = 20) : IRequest<Result<List<Conversation>>>;
