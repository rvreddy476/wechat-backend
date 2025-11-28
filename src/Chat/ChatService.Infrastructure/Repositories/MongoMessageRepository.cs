using ChatService.Domain.Entities;
using ChatService.Domain.Repositories;
using MongoDB.Driver;
using Shared.Domain.Common;

namespace ChatService.Infrastructure.Repositories;

public class MongoMessageRepository : IMessageRepository
{
    private readonly IMongoCollection<Message> _messages;

    public MongoMessageRepository(IMongoDatabase database)
    {
        _messages = database.GetCollection<Message>("messages");
    }

    public async Task<Result<Message>> GetByIdAsync(string messageId)
    {
        try
        {
            var message = await _messages.Find(m => m.Id == messageId && !m.IsDeleted)
                .FirstOrDefaultAsync();

            return message != null
                ? Result.Success(message)
                : Result.Failure<Message>("Message not found");
        }
        catch (Exception ex)
        {
            return Result.Failure<Message>($"Error retrieving message: {ex.Message}");
        }
    }

    public async Task<Result<Message>> CreateAsync(Message message)
    {
        try
        {
            await _messages.InsertOneAsync(message);
            return Result.Success(message);
        }
        catch (Exception ex)
        {
            return Result.Failure<Message>($"Error creating message: {ex.Message}");
        }
    }

    public async Task<Result<Message>> UpdateAsync(Message message)
    {
        try
        {
            message.UpdatedAt = DateTime.UtcNow;
            var result = await _messages.ReplaceOneAsync(
                m => m.Id == message.Id,
                message);

            return result.ModifiedCount > 0
                ? Result.Success(message)
                : Result.Failure<Message>("Message not found");
        }
        catch (Exception ex)
        {
            return Result.Failure<Message>($"Error updating message: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteAsync(string messageId)
    {
        try
        {
            var update = Builders<Message>.Update
                .Set(m => m.IsDeleted, true)
                .Set(m => m.DeletedAt, DateTime.UtcNow);

            var result = await _messages.UpdateOneAsync(
                m => m.Id == messageId,
                update);

            return result.ModifiedCount > 0
                ? Result.Success(true)
                : Result.Failure<bool>("Message not found");
        }
        catch (Exception ex)
        {
            return Result.Failure<bool>($"Error deleting message: {ex.Message}");
        }
    }

    public async Task<Result<List<Message>>> GetConversationMessagesAsync(string conversationId, int page, int pageSize)
    {
        try
        {
            var skip = (page - 1) * pageSize;
            var messages = await _messages
                .Find(m => m.ConversationId == conversationId && !m.IsDeleted)
                .SortByDescending(m => m.CreatedAt)
                .Skip(skip)
                .Limit(pageSize)
                .ToListAsync();

            return Result.Success(messages);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<Message>>($"Error retrieving messages: {ex.Message}");
        }
    }

    public async Task<Result<int>> GetUnreadCountAsync(string conversationId, Guid userId)
    {
        try
        {
            // Get count of messages where user hasn't read
            var count = await _messages.CountDocumentsAsync(m =>
                m.ConversationId == conversationId &&
                !m.IsDeleted &&
                m.SenderId != userId &&
                !m.ReadBy.Any(r => r.UserId == userId));

            return Result.Success((int)count);
        }
        catch (Exception ex)
        {
            return Result.Failure<int>($"Error getting unread count: {ex.Message}");
        }
    }

    public async Task<Result<bool>> MarkMessagesAsReadAsync(string conversationId, Guid userId, List<string> messageIds)
    {
        try
        {
            var filter = Builders<Message>.Filter.And(
                Builders<Message>.Filter.Eq(m => m.ConversationId, conversationId),
                Builders<Message>.Filter.In(m => m.Id, messageIds),
                Builders<Message>.Filter.Not(Builders<Message>.Filter.ElemMatch(
                    m => m.ReadBy,
                    r => r.UserId == userId)));

            var update = Builders<Message>.Update.Push(m => m.ReadBy, new MessageReadReceipt
            {
                UserId = userId,
                ReadAt = DateTime.UtcNow
            });

            await _messages.UpdateManyAsync(filter, update);
            return Result.Success(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool>($"Error marking messages as read: {ex.Message}");
        }
    }
}
