using Chat.Domain.Entities;
using Chat.Domain.Repositories;
using MediatR;
using Shared.Domain.Common;

namespace Chat.Application.Conversations.Queries.GetConversations;

public class GetConversationsQueryHandler : IRequestHandler<GetConversationsQuery, Result<List<Conversation>>>
{
    private readonly IConversationRepository _conversationRepository;

    public GetConversationsQueryHandler(IConversationRepository conversationRepository)
    {
        _conversationRepository = conversationRepository;
    }

    public async Task<Result<List<Conversation>>> Handle(GetConversationsQuery request, CancellationToken cancellationToken)
    {
        return await _conversationRepository.GetUserConversationsAsync(request.UserId, request.Page, request.PageSize);
    }
}
