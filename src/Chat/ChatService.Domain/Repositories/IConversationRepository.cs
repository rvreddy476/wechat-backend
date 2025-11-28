using ChatService.Domain.Entities;
using Shared.Domain.Common;

namespace ChatService.Domain.Repositories;

public interface IConversationRepository
{
    Task<Result<Conversation>> GetByIdAsync(string conversationId);
    Task<Result<Conversation>> CreateAsync(Conversation conversation);
    Task<Result<Conversation>> UpdateAsync(Conversation conversation);
    Task<Result<bool>> DeleteAsync(string conversationId);
    Task<Result<List<Conversation>>> GetUserConversationsAsync(Guid userId, int page, int pageSize);
    Task<Result<Conversation>> GetOneToOneConversationAsync(Guid userId1, Guid userId2);
    Task<Result<bool>> AddParticipantAsync(string conversationId, Participant participant);
    Task<Result<bool>> RemoveParticipantAsync(string conversationId, Guid userId);
    Task<Result<int>> GetUnreadCountAsync(string conversationId, Guid userId);
}
