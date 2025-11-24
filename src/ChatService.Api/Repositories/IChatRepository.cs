using ChatService.Api.Models;
using Shared.Domain.Common;

namespace ChatService.Api.Repositories;

public interface IChatRepository
{
    // Conversation Management
    Task<Result<Conversation>> CreateConversationAsync(Conversation conversation);
    Task<Result<Conversation>> GetConversationByIdAsync(string conversationId);
    Task<Result<Conversation>> GetOneToOneConversationAsync(Guid user1Id, Guid user2Id);
    Task<Result<List<Conversation>>> GetUserConversationsAsync(Guid userId, int page = 1, int pageSize = 20);
    Task<Result<bool>> UpdateConversationAsync(string conversationId, Conversation conversation);
    Task<Result<bool>> DeleteConversationAsync(string conversationId, Guid userId);

    // Group Management
    Task<Result<bool>> AddParticipantAsync(string conversationId, Participant participant);
    Task<Result<bool>> RemoveParticipantAsync(string conversationId, Guid userId);
    Task<Result<bool>> UpdateParticipantLastReadAsync(string conversationId, Guid userId, string lastReadMessageId);
    Task<Result<bool>> MuteConversationAsync(string conversationId, Guid userId, DateTime? mutedUntil);

    // Message Management
    Task<Result<Message>> SendMessageAsync(Message message);
    Task<Result<Message>> GetMessageByIdAsync(string messageId);
    Task<Result<bool>> UpdateMessageAsync(string messageId, Message message);
    Task<Result<bool>> DeleteMessageAsync(string messageId, Guid userId, bool deleteForEveryone = false);
    Task<Result<List<Message>>> GetConversationMessagesAsync(string conversationId, int page = 1, int pageSize = 50);
    Task<Result<List<Message>>> GetMessagesBeforeAsync(string conversationId, DateTime before, int limit = 50);
    Task<Result<List<Message>>> GetMessagesAfterAsync(string conversationId, DateTime after, int limit = 50);

    // Read Receipts
    Task<Result<bool>> MarkMessageAsReadAsync(string messageId, Guid userId);
    Task<Result<bool>> MarkConversationAsReadAsync(string conversationId, Guid userId);
    Task<Result<int>> GetUnreadMessageCountAsync(string conversationId, Guid userId);

    // Search
    Task<Result<List<Message>>> SearchMessagesAsync(string conversationId, string searchTerm, int page = 1, int pageSize = 20);
}
