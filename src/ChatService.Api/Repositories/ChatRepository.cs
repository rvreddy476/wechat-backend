using MongoDB.Driver;
using ChatService.Api.Models;
using Shared.Domain.Common;

namespace ChatService.Api.Repositories;

public class ChatRepository : IChatRepository
{
    private readonly IMongoCollection<Conversation> _conversations;
    private readonly IMongoCollection<Message> _messages;
    private readonly ILogger<ChatRepository> _logger;

    public ChatRepository(IMongoDatabase database, ILogger<ChatRepository> logger)
    {
        _conversations = database.GetCollection<Conversation>("conversations");
        _messages = database.GetCollection<Message>("messages");
        _logger = logger;
    }

    public async Task<Result<Conversation>> CreateConversationAsync(Conversation conversation)
    {
        try
        {
            await _conversations.InsertOneAsync(conversation);
            return Result<Conversation>.Success(conversation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating conversation");
            return Result.Failure<Conversation>("Failed to create conversation");
        }
    }

    public async Task<Result<Conversation>> GetConversationByIdAsync(string conversationId)
    {
        try
        {
            var conversation = await _conversations.Find(c => c.Id == conversationId && !c.IsDeleted)
                .FirstOrDefaultAsync();

            if (conversation == null)
            {
                return Result.Failure<Conversation>("Conversation not found");
            }

            return Result<Conversation>.Success(conversation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting conversation {ConversationId}", conversationId);
            return Result.Failure<Conversation>("Failed to get conversation");
        }
    }

    public async Task<Result<Conversation>> GetOneToOneConversationAsync(Guid user1Id, Guid user2Id)
    {
        try
        {
            var conversation = await _conversations.Find(c =>
                c.Type == ConversationType.OneToOne &&
                c.Participants.Any(p => p.UserId == user1Id) &&
                c.Participants.Any(p => p.UserId == user2Id) &&
                !c.IsDeleted
            ).FirstOrDefaultAsync();

            if (conversation == null)
            {
                return Result.Failure<Conversation>("Conversation not found");
            }

            return Result<Conversation>.Success(conversation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting one-to-one conversation between {User1} and {User2}", user1Id, user2Id);
            return Result.Failure<Conversation>("Failed to get conversation");
        }
    }

    public async Task<Result<List<Conversation>>> GetUserConversationsAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        try
        {
            var conversations = await _conversations.Find(c =>
                c.Participants.Any(p => p.UserId == userId) &&
                !c.IsDeleted
            )
                .SortByDescending(c => c.UpdatedAt)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            return Result<List<Conversation>>.Success(conversations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting conversations for user {UserId}", userId);
            return Result.Failure<List<Conversation>>("Failed to get conversations");
        }
    }

    public async Task<Result<bool>> UpdateConversationAsync(string conversationId, Conversation conversation)
    {
        try
        {
            conversation.UpdatedAt = DateTime.UtcNow;

            var result = await _conversations.ReplaceOneAsync(
                c => c.Id == conversationId && !c.IsDeleted,
                conversation
            );

            if (result.MatchedCount == 0)
            {
                return Result.Failure<bool>("Conversation not found");
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating conversation {ConversationId}", conversationId);
            return Result.Failure<bool>("Failed to update conversation");
        }
    }

    public async Task<Result<bool>> DeleteConversationAsync(string conversationId, Guid userId)
    {
        try
        {
            // For one-to-one conversations, just mark as deleted for this user
            // For group conversations, remove the participant

            var conversation = await _conversations.Find(c => c.Id == conversationId && !c.IsDeleted)
                .FirstOrDefaultAsync();

            if (conversation == null)
            {
                return Result.Failure<bool>("Conversation not found");
            }

            if (conversation.Type == ConversationType.OneToOne)
            {
                // Soft delete for the user
                var update = Builders<Conversation>.Update
                    .Set(c => c.IsDeleted, true)
                    .Set(c => c.DeletedAt, DateTime.UtcNow)
                    .Set(c => c.UpdatedAt, DateTime.UtcNow);

                await _conversations.UpdateOneAsync(c => c.Id == conversationId, update);
            }
            else
            {
                // Remove participant from group
                await RemoveParticipantAsync(conversationId, userId);
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting conversation {ConversationId}", conversationId);
            return Result.Failure<bool>("Failed to delete conversation");
        }
    }

    public async Task<Result<bool>> AddParticipantAsync(string conversationId, Participant participant)
    {
        try
        {
            var update = Builders<Conversation>.Update
                .Push(c => c.Participants, participant)
                .Set(c => c.UpdatedAt, DateTime.UtcNow);

            var result = await _conversations.UpdateOneAsync(
                c => c.Id == conversationId && !c.IsDeleted,
                update
            );

            if (result.MatchedCount == 0)
            {
                return Result.Failure<bool>("Conversation not found");
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding participant to conversation {ConversationId}", conversationId);
            return Result.Failure<bool>("Failed to add participant");
        }
    }

    public async Task<Result<bool>> RemoveParticipantAsync(string conversationId, Guid userId)
    {
        try
        {
            var update = Builders<Conversation>.Update
                .PullFilter(c => c.Participants, p => p.UserId == userId)
                .Set(c => c.UpdatedAt, DateTime.UtcNow);

            var result = await _conversations.UpdateOneAsync(
                c => c.Id == conversationId && !c.IsDeleted,
                update
            );

            if (result.MatchedCount == 0)
            {
                return Result.Failure<bool>("Conversation not found");
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing participant from conversation {ConversationId}", conversationId);
            return Result.Failure<bool>("Failed to remove participant");
        }
    }

    public async Task<Result<bool>> UpdateParticipantLastReadAsync(string conversationId, Guid userId, string lastReadMessageId)
    {
        try
        {
            var filter = Builders<Conversation>.Filter.And(
                Builders<Conversation>.Filter.Eq(c => c.Id, conversationId),
                Builders<Conversation>.Filter.ElemMatch(c => c.Participants, p => p.UserId == userId)
            );

            var update = Builders<Conversation>.Update
                .Set("participants.$.lastReadMessageId", lastReadMessageId)
                .Set("participants.$.lastReadAt", DateTime.UtcNow)
                .Set(c => c.UpdatedAt, DateTime.UtcNow);

            var result = await _conversations.UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0)
            {
                return Result.Failure<bool>("Conversation or participant not found");
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating last read for user {UserId} in conversation {ConversationId}", userId, conversationId);
            return Result.Failure<bool>("Failed to update last read");
        }
    }

    public async Task<Result<bool>> MuteConversationAsync(string conversationId, Guid userId, DateTime? mutedUntil)
    {
        try
        {
            var filter = Builders<Conversation>.Filter.And(
                Builders<Conversation>.Filter.Eq(c => c.Id, conversationId),
                Builders<Conversation>.Filter.ElemMatch(c => c.Participants, p => p.UserId == userId)
            );

            var update = Builders<Conversation>.Update
                .Set("participants.$.isMuted", mutedUntil != null)
                .Set("participants.$.mutedUntil", mutedUntil)
                .Set(c => c.UpdatedAt, DateTime.UtcNow);

            var result = await _conversations.UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0)
            {
                return Result.Failure<bool>("Conversation or participant not found");
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error muting conversation {ConversationId} for user {UserId}", conversationId, userId);
            return Result.Failure<bool>("Failed to mute conversation");
        }
    }

    public async Task<Result<Message>> SendMessageAsync(Message message)
    {
        try
        {
            await _messages.InsertOneAsync(message);

            // Update conversation's last message
            var lastMessage = new LastMessage
            {
                MessageId = message.Id,
                SenderId = message.SenderId,
                SenderUsername = message.SenderUsername,
                Content = message.Content,
                MessageType = message.MessageType,
                SentAt = message.CreatedAt
            };

            var update = Builders<Conversation>.Update
                .Set(c => c.LastMessage, lastMessage)
                .Set(c => c.UpdatedAt, DateTime.UtcNow);

            await _conversations.UpdateOneAsync(c => c.Id == message.ConversationId, update);

            return Result<Message>.Success(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message to conversation {ConversationId}", message.ConversationId);
            return Result.Failure<Message>("Failed to send message");
        }
    }

    public async Task<Result<Message>> GetMessageByIdAsync(string messageId)
    {
        try
        {
            var message = await _messages.Find(m => m.Id == messageId && !m.IsDeleted)
                .FirstOrDefaultAsync();

            if (message == null)
            {
                return Result.Failure<Message>("Message not found");
            }

            return Result<Message>.Success(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting message {MessageId}", messageId);
            return Result.Failure<Message>("Failed to get message");
        }
    }

    public async Task<Result<bool>> UpdateMessageAsync(string messageId, Message message)
    {
        try
        {
            message.IsEdited = true;
            message.EditedAt = DateTime.UtcNow;
            message.UpdatedAt = DateTime.UtcNow;

            var result = await _messages.ReplaceOneAsync(
                m => m.Id == messageId && !m.IsDeleted,
                message
            );

            if (result.MatchedCount == 0)
            {
                return Result.Failure<bool>("Message not found");
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating message {MessageId}", messageId);
            return Result.Failure<bool>("Failed to update message");
        }
    }

    public async Task<Result<bool>> DeleteMessageAsync(string messageId, Guid userId, bool deleteForEveryone = false)
    {
        try
        {
            if (deleteForEveryone)
            {
                // Hard delete (set isDeleted flag)
                var update = Builders<Message>.Update
                    .Set(m => m.IsDeleted, true)
                    .Set(m => m.DeletedAt, DateTime.UtcNow)
                    .Set(m => m.UpdatedAt, DateTime.UtcNow);

                await _messages.UpdateOneAsync(m => m.Id == messageId && m.SenderId == userId, update);
            }
            else
            {
                // Soft delete (delete for this user only)
                var update = Builders<Message>.Update
                    .AddToSet(m => m.DeletedFor, userId)
                    .Set(m => m.UpdatedAt, DateTime.UtcNow);

                await _messages.UpdateOneAsync(m => m.Id == messageId, update);
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting message {MessageId}", messageId);
            return Result.Failure<bool>("Failed to delete message");
        }
    }

    public async Task<Result<List<Message>>> GetConversationMessagesAsync(string conversationId, int page = 1, int pageSize = 50)
    {
        try
        {
            var messages = await _messages.Find(m => m.ConversationId == conversationId && !m.IsDeleted)
                .SortByDescending(m => m.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            // Reverse to get chronological order
            messages.Reverse();

            return Result<List<Message>>.Success(messages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting messages for conversation {ConversationId}", conversationId);
            return Result.Failure<List<Message>>("Failed to get messages");
        }
    }

    public async Task<Result<List<Message>>> GetMessagesBeforeAsync(string conversationId, DateTime before, int limit = 50)
    {
        try
        {
            var messages = await _messages.Find(m =>
                m.ConversationId == conversationId &&
                m.CreatedAt < before &&
                !m.IsDeleted
            )
                .SortByDescending(m => m.CreatedAt)
                .Limit(limit)
                .ToListAsync();

            messages.Reverse();

            return Result<List<Message>>.Success(messages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting messages before {Before} for conversation {ConversationId}", before, conversationId);
            return Result.Failure<List<Message>>("Failed to get messages");
        }
    }

    public async Task<Result<List<Message>>> GetMessagesAfterAsync(string conversationId, DateTime after, int limit = 50)
    {
        try
        {
            var messages = await _messages.Find(m =>
                m.ConversationId == conversationId &&
                m.CreatedAt > after &&
                !m.IsDeleted
            )
                .SortBy(m => m.CreatedAt)
                .Limit(limit)
                .ToListAsync();

            return Result<List<Message>>.Success(messages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting messages after {After} for conversation {ConversationId}", after, conversationId);
            return Result.Failure<List<Message>>("Failed to get messages");
        }
    }

    public async Task<Result<bool>> MarkMessageAsReadAsync(string messageId, Guid userId)
    {
        try
        {
            var readReceipt = new MessageReadReceipt
            {
                UserId = userId,
                ReadAt = DateTime.UtcNow
            };

            var update = Builders<Message>.Update
                .AddToSet(m => m.ReadBy, readReceipt)
                .Set(m => m.UpdatedAt, DateTime.UtcNow);

            await _messages.UpdateOneAsync(m => m.Id == messageId, update);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking message {MessageId} as read for user {UserId}", messageId, userId);
            return Result.Failure<bool>("Failed to mark message as read");
        }
    }

    public async Task<Result<bool>> MarkConversationAsReadAsync(string conversationId, Guid userId)
    {
        try
        {
            // Get the last message in the conversation
            var lastMessage = await _messages.Find(m => m.ConversationId == conversationId && !m.IsDeleted)
                .SortByDescending(m => m.CreatedAt)
                .Limit(1)
                .FirstOrDefaultAsync();

            if (lastMessage != null)
            {
                await UpdateParticipantLastReadAsync(conversationId, userId, lastMessage.Id);
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking conversation {ConversationId} as read for user {UserId}", conversationId, userId);
            return Result.Failure<bool>("Failed to mark conversation as read");
        }
    }

    public async Task<Result<int>> GetUnreadMessageCountAsync(string conversationId, Guid userId)
    {
        try
        {
            var conversation = await _conversations.Find(c => c.Id == conversationId && !c.IsDeleted)
                .FirstOrDefaultAsync();

            if (conversation == null)
            {
                return Result.Failure<int>("Conversation not found");
            }

            var participant = conversation.Participants.FirstOrDefault(p => p.UserId == userId);
            if (participant == null)
            {
                return Result.Failure<int>("User is not a participant");
            }

            var lastReadAt = participant.LastReadAt ?? DateTime.MinValue;

            var unreadCount = await _messages.CountDocumentsAsync(m =>
                m.ConversationId == conversationId &&
                m.SenderId != userId &&
                m.CreatedAt > lastReadAt &&
                !m.IsDeleted
            );

            return Result<int>.Success((int)unreadCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread count for conversation {ConversationId}", conversationId);
            return Result.Failure<int>("Failed to get unread count");
        }
    }

    public async Task<Result<List<Message>>> SearchMessagesAsync(string conversationId, string searchTerm, int page = 1, int pageSize = 20)
    {
        try
        {
            var filter = Builders<Message>.Filter.And(
                Builders<Message>.Filter.Eq(m => m.ConversationId, conversationId),
                Builders<Message>.Filter.Eq(m => m.IsDeleted, false),
                Builders<Message>.Filter.Regex(m => m.Content, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"))
            );

            var messages = await _messages.Find(filter)
                .SortByDescending(m => m.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            return Result<List<Message>>.Success(messages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching messages in conversation {ConversationId}", conversationId);
            return Result.Failure<List<Message>>("Failed to search messages");
        }
    }
}
