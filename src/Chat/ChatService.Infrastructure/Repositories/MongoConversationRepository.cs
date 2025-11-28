using ChatService.Domain.Entities;
using ChatService.Domain.Repositories;
using MongoDB.Driver;
using Shared.Domain.Common;

namespace ChatService.Infrastructure.Repositories;

public class MongoConversationRepository : IConversationRepository
{
    private readonly IMongoCollection<Conversation> _conversations;

    public MongoConversationRepository(IMongoDatabase database)
    {
        _conversations = database.GetCollection<Conversation>("conversations");
    }

    public async Task<Result<Conversation>> GetByIdAsync(string conversationId)
    {
        try
        {
            var conversation = await _conversations.Find(c => c.Id == conversationId && !c.IsDeleted)
                .FirstOrDefaultAsync();

            return conversation != null
                ? Result.Success(conversation)
                : Result.Failure<Conversation>("Conversation not found");
        }
        catch (Exception ex)
        {
            return Result.Failure<Conversation>($"Error retrieving conversation: {ex.Message}");
        }
    }

    public async Task<Result<Conversation>> CreateAsync(Conversation conversation)
    {
        try
        {
            await _conversations.InsertOneAsync(conversation);
            return Result.Success(conversation);
        }
        catch (Exception ex)
        {
            return Result.Failure<Conversation>($"Error creating conversation: {ex.Message}");
        }
    }

    public async Task<Result<Conversation>> UpdateAsync(Conversation conversation)
    {
        try
        {
            conversation.UpdatedAt = DateTime.UtcNow;
            var result = await _conversations.ReplaceOneAsync(
                c => c.Id == conversation.Id,
                conversation);

            return result.ModifiedCount > 0
                ? Result.Success(conversation)
                : Result.Failure<Conversation>("Conversation not found");
        }
        catch (Exception ex)
        {
            return Result.Failure<Conversation>($"Error updating conversation: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteAsync(string conversationId)
    {
        try
        {
            var update = Builders<Conversation>.Update
                .Set(c => c.IsDeleted, true)
                .Set(c => c.DeletedAt, DateTime.UtcNow);

            var result = await _conversations.UpdateOneAsync(
                c => c.Id == conversationId,
                update);

            return result.ModifiedCount > 0
                ? Result.Success(true)
                : Result.Failure<bool>("Conversation not found");
        }
        catch (Exception ex)
        {
            return Result.Failure<bool>($"Error deleting conversation: {ex.Message}");
        }
    }

    public async Task<Result<List<Conversation>>> GetUserConversationsAsync(Guid userId, int page, int pageSize)
    {
        try
        {
            var skip = (page - 1) * pageSize;
            var conversations = await _conversations
                .Find(c => c.Participants.Any(p => p.UserId == userId) && !c.IsDeleted)
                .SortByDescending(c => c.UpdatedAt)
                .Skip(skip)
                .Limit(pageSize)
                .ToListAsync();

            return Result.Success(conversations);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<Conversation>>($"Error retrieving conversations: {ex.Message}");
        }
    }

    public async Task<Result<Conversation>> GetOneToOneConversationAsync(Guid userId1, Guid userId2)
    {
        try
        {
            var conversation = await _conversations.Find(c =>
                c.Type == ConversationType.OneToOne &&
                c.Participants.Any(p => p.UserId == userId1) &&
                c.Participants.Any(p => p.UserId == userId2) &&
                !c.IsDeleted
            ).FirstOrDefaultAsync();

            return conversation != null
                ? Result.Success(conversation)
                : Result.Failure<Conversation>("Conversation not found");
        }
        catch (Exception ex)
        {
            return Result.Failure<Conversation>($"Error retrieving conversation: {ex.Message}");
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
                c => c.Id == conversationId,
                update);

            return result.ModifiedCount > 0
                ? Result.Success(true)
                : Result.Failure<bool>("Conversation not found");
        }
        catch (Exception ex)
        {
            return Result.Failure<bool>($"Error adding participant: {ex.Message}");
        }
    }

    public async Task<Result<bool>> RemoveParticipantAsync(string conversationId, Guid userId)
    {
        try
        {
            var conversation = await GetByIdAsync(conversationId);
            if (!conversation.IsSuccess)
                return Result.Failure<bool>("Conversation not found");

            var conv = conversation.Value;
            conv.RemoveParticipant(userId);

            return await UpdateAsync(conv).ContinueWith(t => Result.Success(true));
        }
        catch (Exception ex)
        {
            return Result.Failure<bool>($"Error removing participant: {ex.Message}");
        }
    }

    public async Task<Result<int>> GetUnreadCountAsync(string conversationId, Guid userId)
    {
        try
        {
            var conversation = await GetByIdAsync(conversationId);
            if (!conversation.IsSuccess)
                return Result.Failure<int>("Conversation not found");

            var participant = conversation.Value.Participants.FirstOrDefault(p => p.UserId == userId);
            if (participant == null)
                return Result.Success(0);

            // This would need to query the messages collection
            // For now, return 0
            return Result.Success(0);
        }
        catch (Exception ex)
        {
            return Result.Failure<int>($"Error getting unread count: {ex.Message}");
        }
    }
}
