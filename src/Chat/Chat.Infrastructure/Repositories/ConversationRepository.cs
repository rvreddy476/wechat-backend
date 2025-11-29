using Chat.Domain.Entities;
using Chat.Domain.Repositories;
using MongoDB.Driver;
using Shared.Domain.Common;

namespace Chat.Infrastructure.Repositories;

public class ConversationRepository : IConversationRepository
{
    private readonly IMongoCollection<Conversation> _conversations;

    public ConversationRepository(IMongoDatabase database)
    {
        _conversations = database.GetCollection<Conversation>("conversations");
    }

    public async Task<Result<Conversation>> GetByIdAsync(string conversationId)
    {
        try
        {
            var conversation = await _conversations.Find(c => c.Id == conversationId && !c.IsDeleted).FirstOrDefaultAsync();
            return conversation != null ? Result.Success(conversation) : Result.Failure<Conversation>("Conversation not found");
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
            var result = await _conversations.ReplaceOneAsync(c => c.Id == conversation.Id, conversation);
            return result.ModifiedCount > 0 ? Result.Success(conversation) : Result.Failure<Conversation>("Conversation not found");
        }
        catch (Exception ex)
        {
            return Result.Failure<Conversation>($"Error updating conversation: {ex.Message}");
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

            return conversation != null ? Result.Success(conversation) : Result.Failure<Conversation>("Conversation not found");
        }
        catch (Exception ex)
        {
            return Result.Failure<Conversation>($"Error retrieving conversation: {ex.Message}");
        }
    }
}
