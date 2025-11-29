using Chat.Domain.Entities;
using Chat.Domain.Repositories;
using MediatR;
using Shared.Domain.Common;

namespace Chat.Application.Messages.Queries.GetMessages;

public class GetMessagesQueryHandler : IRequestHandler<GetMessagesQuery, Result<List<Message>>>
{
    private readonly IMessageRepository _messageRepository;

    public GetMessagesQueryHandler(IMessageRepository messageRepository) => _messageRepository = messageRepository;

    public async Task<Result<List<Message>>> Handle(GetMessagesQuery request, CancellationToken cancellationToken)
    {
        return await _messageRepository.GetConversationMessagesAsync(request.ConversationId, request.Page, request.PageSize);
    }
}
