using ChatService.Domain.Entities;
using Shared.Domain.Common;

namespace ChatService.Domain.Repositories;

public interface IMessageRepository
{
    Task<Result<Message>> GetByIdAsync(string messageId);
    Task<Result<Message>> CreateAsync(Message message);
    Task<Result<Message>> UpdateAsync(Message message);
    Task<Result<bool>> DeleteAsync(string messageId);
    Task<Result<List<Message>>> GetConversationMessagesAsync(string conversationId, int page, int pageSize);
    Task<Result<int>> GetUnreadCountAsync(string conversationId, Guid userId);
    Task<Result<bool>> MarkMessagesAsReadAsync(string conversationId, Guid userId, List<string> messageIds);
}
