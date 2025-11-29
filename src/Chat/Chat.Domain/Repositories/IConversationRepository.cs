using Chat.Domain.Entities;
using Shared.Domain.Common;

namespace Chat.Domain.Repositories;

public interface IConversationRepository
{
    Task<Result<Conversation>> GetByIdAsync(string conversationId);
    Task<Result<Conversation>> CreateAsync(Conversation conversation);
    Task<Result<Conversation>> UpdateAsync(Conversation conversation);
    Task<Result<List<Conversation>>> GetUserConversationsAsync(Guid userId, int page, int pageSize);
    Task<Result<Conversation>> GetOneToOneConversationAsync(Guid userId1, Guid userId2);
}
