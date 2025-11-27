using MongoDB.Driver;
using PostFeedService.Api.Models;
using Shared.Domain.Common;

namespace PostFeedService.Api.Repositories;

public class PostRepository : IPostRepository
{
    private readonly IMongoCollection<Post> _posts;
    private readonly IMongoCollection<Comment> _comments;
    private readonly IMongoCollection<Reaction> _reactions;
    private readonly IMongoCollection<Hashtag> _hashtags;
    private readonly ILogger<PostRepository> _logger;

    public PostRepository(IMongoDatabase database, ILogger<PostRepository> logger)
    {
        _posts = database.GetCollection<Post>("posts");
        _comments = database.GetCollection<Comment>("comments");
        _reactions = database.GetCollection<Reaction>("reactions");
        _hashtags = database.GetCollection<Hashtag>("hashtags");
        _logger = logger;
    }

    public async Task<Result<Post>> CreatePostAsync(Post post)
    {
        try
        {
            await _posts.InsertOneAsync(post);

            // Update hashtag usage
            if (post.Hashtags.Any())
            {
                await UpdateHashtagUsageAsync(post.Hashtags);
            }

            return Result<Post>.Success(post);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating post for user {UserId}", post.UserId);
            return Result.Failure<Post>("Failed to create post");
        }
    }

    public async Task<Result<Post>> GetPostByIdAsync(string postId)
    {
        try
        {
            var post = await _posts.Find(p => p.Id == postId && !p.IsDeleted)
                .FirstOrDefaultAsync();

            if (post == null)
            {
                return Result.Failure<Post>("Post not found");
            }

            return Result<Post>.Success(post);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting post {PostId}", postId);
            return Result.Failure<Post>("Failed to get post");
        }
    }

    public async Task<Result<bool>> UpdatePostAsync(string postId, Post post)
    {
        try
        {
            post.UpdatedAt = DateTime.UtcNow;

            var result = await _posts.ReplaceOneAsync(
                p => p.Id == postId && !p.IsDeleted,
                post
            );

            if (result.MatchedCount == 0)
            {
                return Result.Failure<bool>("Post not found");
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating post {PostId}", postId);
            return Result.Failure<bool>("Failed to update post");
        }
    }

    public async Task<Result<bool>> DeletePostAsync(string postId, Guid userId)
    {
        try
        {
            var update = Builders<Post>.Update
                .Set(p => p.IsDeleted, true)
                .Set(p => p.DeletedAt, DateTime.UtcNow)
                .Set(p => p.UpdatedAt, DateTime.UtcNow);

            var result = await _posts.UpdateOneAsync(
                p => p.Id == postId && p.UserId == userId && !p.IsDeleted,
                update
            );

            if (result.MatchedCount == 0)
            {
                return Result.Failure<bool>("Post not found or unauthorized");
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting post {PostId}", postId);
            return Result.Failure<bool>("Failed to delete post");
        }
    }

    public async Task<Result<List<Post>>> GetUserPostsAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        try
        {
            var posts = await _posts.Find(p => p.UserId == userId && !p.IsDeleted)
                .SortByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            return Result<List<Post>>.Success(posts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting posts for user {UserId}", userId);
            return Result.Failure<List<Post>>("Failed to get user posts");
        }
    }

    public async Task<Result<List<Post>>> GetTimelineFeedAsync(Guid userId, List<Guid> followingIds, int page = 1, int pageSize = 20)
    {
        try
        {
            var userIds = new List<Guid>(followingIds) { userId };

            var posts = await _posts.Find(p => userIds.Contains(p.UserId) && !p.IsDeleted)
                .SortByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            return Result<List<Post>>.Success(posts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting timeline feed for user {UserId}", userId);
            return Result.Failure<List<Post>>("Failed to get timeline feed");
        }
    }

    public async Task<Result<List<Post>>> GetExploreFeedAsync(int page = 1, int pageSize = 20)
    {
        try
        {
            var posts = await _posts.Find(p => p.Visibility == PostVisibility.Public && !p.IsDeleted)
                .SortByDescending(p => p.Stats.ViewsCount)
                .ThenByDescending(p => p.Stats.LikesCount)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            return Result<List<Post>>.Success(posts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting explore feed");
            return Result.Failure<List<Post>>("Failed to get explore feed");
        }
    }

    public async Task<Result<List<Post>>> GetTrendingPostsAsync(int limit = 10)
    {
        try
        {
            var yesterday = DateTime.UtcNow.AddDays(-1);

            var posts = await _posts.Find(p => p.Visibility == PostVisibility.Public && !p.IsDeleted && p.CreatedAt >= yesterday)
                .SortByDescending(p => p.Stats.LikesCount + p.Stats.CommentsCount + p.Stats.SharesCount)
                .Limit(limit)
                .ToListAsync();

            return Result<List<Post>>.Success(posts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting trending posts");
            return Result.Failure<List<Post>>("Failed to get trending posts");
        }
    }

    public async Task<Result<bool>> IncrementViewCountAsync(string postId)
    {
        try
        {
            var update = Builders<Post>.Update
                .Inc(p => p.Stats.ViewsCount, 1)
                .Set(p => p.UpdatedAt, DateTime.UtcNow);

            await _posts.UpdateOneAsync(p => p.Id == postId, update);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing view count for post {PostId}", postId);
            return Result.Failure<bool>("Failed to increment view count");
        }
    }

    public async Task<Result<bool>> UpdatePostStatsAsync(string postId, string statField, int incrementBy)
    {
        try
        {
            var update = Builders<Post>.Update
                .Inc($"stats.{statField}", incrementBy)
                .Set(p => p.UpdatedAt, DateTime.UtcNow);

            var result = await _posts.UpdateOneAsync(
                p => p.Id == postId && !p.IsDeleted,
                update
            );

            if (result.MatchedCount == 0)
            {
                return Result.Failure<bool>("Post not found");
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating post stats for {PostId}", postId);
            return Result.Failure<bool>("Failed to update post stats");
        }
    }

    public async Task<Result<Comment>> CreateCommentAsync(Comment comment)
    {
        try
        {
            await _comments.InsertOneAsync(comment);

            // Increment post comment count
            await UpdatePostStatsAsync(comment.PostId, "commentsCount", 1);

            // If it's a reply, increment parent comment reply count
            if (!string.IsNullOrEmpty(comment.ParentCommentId))
            {
                var updateParent = Builders<Comment>.Update
                    .Inc(c => c.RepliesCount, 1)
                    .Set(c => c.UpdatedAt, DateTime.UtcNow);

                await _comments.UpdateOneAsync(c => c.Id == comment.ParentCommentId, updateParent);
            }

            return Result<Comment>.Success(comment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating comment for post {PostId}", comment.PostId);
            return Result.Failure<Comment>("Failed to create comment");
        }
    }

    public async Task<Result<Comment>> GetCommentByIdAsync(string commentId)
    {
        try
        {
            var comment = await _comments.Find(c => c.Id == commentId && !c.IsDeleted)
                .FirstOrDefaultAsync();

            if (comment == null)
            {
                return Result.Failure<Comment>("Comment not found");
            }

            return Result<Comment>.Success(comment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting comment {CommentId}", commentId);
            return Result.Failure<Comment>("Failed to get comment");
        }
    }

    public async Task<Result<bool>> UpdateCommentAsync(string commentId, Comment comment)
    {
        try
        {
            comment.UpdatedAt = DateTime.UtcNow;

            var result = await _comments.ReplaceOneAsync(
                c => c.Id == commentId && !c.IsDeleted,
                comment
            );

            if (result.MatchedCount == 0)
            {
                return Result.Failure<bool>("Comment not found");
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating comment {CommentId}", commentId);
            return Result.Failure<bool>("Failed to update comment");
        }
    }

    public async Task<Result<bool>> DeleteCommentAsync(string commentId, Guid userId)
    {
        try
        {
            var comment = await _comments.Find(c => c.Id == commentId && !c.IsDeleted)
                .FirstOrDefaultAsync();

            if (comment == null || comment.UserId != userId)
            {
                return Result.Failure<bool>("Comment not found or unauthorized");
            }

            var update = Builders<Comment>.Update
                .Set(c => c.IsDeleted, true)
                .Set(c => c.DeletedAt, DateTime.UtcNow)
                .Set(c => c.UpdatedAt, DateTime.UtcNow);

            await _comments.UpdateOneAsync(c => c.Id == commentId, update);

            // Decrement post comment count
            await UpdatePostStatsAsync(comment.PostId, "commentsCount", -1);

            // If it's a reply, decrement parent comment reply count
            if (!string.IsNullOrEmpty(comment.ParentCommentId))
            {
                var updateParent = Builders<Comment>.Update
                    .Inc(c => c.RepliesCount, -1)
                    .Set(c => c.UpdatedAt, DateTime.UtcNow);

                await _comments.UpdateOneAsync(c => c.Id == comment.ParentCommentId, updateParent);
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting comment {CommentId}", commentId);
            return Result.Failure<bool>("Failed to delete comment");
        }
    }

    public async Task<Result<List<Comment>>> GetPostCommentsAsync(string postId, int page = 1, int pageSize = 20)
    {
        try
        {
            var comments = await _comments.Find(c => c.PostId == postId && c.Level == 0 && !c.IsDeleted)
                .SortByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            return Result<List<Comment>>.Success(comments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting comments for post {PostId}", postId);
            return Result.Failure<List<Comment>>("Failed to get comments");
        }
    }

    public async Task<Result<List<Comment>>> GetCommentRepliesAsync(string parentCommentId, int page = 1, int pageSize = 10)
    {
        try
        {
            var replies = await _comments.Find(c => c.ParentCommentId == parentCommentId && !c.IsDeleted)
                .SortBy(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            return Result<List<Comment>>.Success(replies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting replies for comment {CommentId}", parentCommentId);
            return Result.Failure<List<Comment>>("Failed to get replies");
        }
    }

    public async Task<Result<Reaction>> AddReactionAsync(Reaction reaction)
    {
        try
        {
            // Check if user already reacted
            var existing = await _reactions.Find(r =>
                r.TargetId == reaction.TargetId &&
                r.UserId == reaction.UserId &&
                r.TargetType == reaction.TargetType
            ).FirstOrDefaultAsync();

            if (existing != null)
            {
                // Update existing reaction
                existing.ReactionType = reaction.ReactionType;
                existing.UpdatedAt = DateTime.UtcNow;

                await _reactions.ReplaceOneAsync(r => r.Id == existing.Id, existing);
                return Result<Reaction>.Success(existing);
            }

            // Create new reaction
            await _reactions.InsertOneAsync(reaction);

            // Update stats
            if (reaction.TargetType == ReactionTargetType.Post)
            {
                await UpdatePostStatsAsync(reaction.TargetId, "likesCount", 1);
            }
            else if (reaction.TargetType == ReactionTargetType.Comment)
            {
                var updateComment = Builders<Comment>.Update
                    .Inc(c => c.LikesCount, 1)
                    .Set(c => c.UpdatedAt, DateTime.UtcNow);

                await _comments.UpdateOneAsync(c => c.Id == reaction.TargetId, updateComment);
            }

            return Result<Reaction>.Success(reaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding reaction to {TargetType} {TargetId}", reaction.TargetType, reaction.TargetId);
            return Result.Failure<Reaction>("Failed to add reaction");
        }
    }

    public async Task<Result<bool>> RemoveReactionAsync(string targetId, Guid userId, ReactionTargetType targetType)
    {
        try
        {
            var result = await _reactions.DeleteOneAsync(r =>
                r.TargetId == targetId &&
                r.UserId == userId &&
                r.TargetType == targetType
            );

            if (result.DeletedCount == 0)
            {
                return Result.Failure<bool>("Reaction not found");
            }

            // Update stats
            if (targetType == ReactionTargetType.Post)
            {
                await UpdatePostStatsAsync(targetId, "likesCount", -1);
            }
            else if (targetType == ReactionTargetType.Comment)
            {
                var updateComment = Builders<Comment>.Update
                    .Inc(c => c.LikesCount, -1)
                    .Set(c => c.UpdatedAt, DateTime.UtcNow);

                await _comments.UpdateOneAsync(c => c.Id == targetId, updateComment);
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing reaction from {TargetType} {TargetId}", targetType, targetId);
            return Result.Failure<bool>("Failed to remove reaction");
        }
    }

    public async Task<Result<Reaction>> GetUserReactionAsync(string targetId, Guid userId, ReactionTargetType targetType)
    {
        try
        {
            var reaction = await _reactions.Find(r =>
                r.TargetId == targetId &&
                r.UserId == userId &&
                r.TargetType == targetType
            ).FirstOrDefaultAsync();

            if (reaction == null)
            {
                return Result.Failure<Reaction>("No reaction found");
            }

            return Result<Reaction>.Success(reaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user reaction for {TargetType} {TargetId}", targetType, targetId);
            return Result.Failure<Reaction>("Failed to get reaction");
        }
    }

    public async Task<Result<List<Reaction>>> GetReactionsAsync(string targetId, ReactionTargetType targetType, int page = 1, int pageSize = 50)
    {
        try
        {
            var reactions = await _reactions.Find(r => r.TargetId == targetId && r.TargetType == targetType)
                .SortByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            return Result<List<Reaction>>.Success(reactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reactions for {TargetType} {TargetId}", targetType, targetId);
            return Result.Failure<List<Reaction>>("Failed to get reactions");
        }
    }

    public async Task<Result<Dictionary<ReactionType, int>>> GetReactionCountsAsync(string targetId, ReactionTargetType targetType)
    {
        try
        {
            var reactions = await _reactions.Find(r => r.TargetId == targetId && r.TargetType == targetType)
                .ToListAsync();

            var counts = reactions
                .GroupBy(r => r.ReactionType)
                .ToDictionary(g => g.Key, g => g.Count());

            return Result<Dictionary<ReactionType, int>>.Success(counts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reaction counts for {TargetType} {TargetId}", targetType, targetId);
            return Result.Failure<Dictionary<ReactionType, int>>("Failed to get reaction counts");
        }
    }

    public async Task<Result<List<Post>>> GetPostsByHashtagAsync(string hashtag, int page = 1, int pageSize = 20)
    {
        try
        {
            var normalizedHashtag = hashtag.ToLowerInvariant().TrimStart('#');

            var posts = await _posts.Find(p =>
                p.Hashtags.Any(h => h.ToLowerInvariant() == normalizedHashtag) &&
                p.Visibility == PostVisibility.Public &&
                !p.IsDeleted
            )
                .SortByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            return Result<List<Post>>.Success(posts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting posts by hashtag {Hashtag}", hashtag);
            return Result.Failure<List<Post>>("Failed to get posts by hashtag");
        }
    }

    public async Task<Result<List<Hashtag>>> GetTrendingHashtagsAsync(int limit = 10)
    {
        try
        {
            var hashtags = await _hashtags.Find(_ => true)
                .SortByDescending(h => h.TrendingScore)
                .Limit(limit)
                .ToListAsync();

            return Result<List<Hashtag>>.Success(hashtags);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting trending hashtags");
            return Result.Failure<List<Hashtag>>("Failed to get trending hashtags");
        }
    }

    public async Task<Result<bool>> UpdateHashtagUsageAsync(List<string> hashtags)
    {
        try
        {
            foreach (var tag in hashtags)
            {
                var normalizedTag = tag.ToLowerInvariant().TrimStart('#');

                var filter = Builders<Hashtag>.Filter.Eq(h => h.NormalizedTag, normalizedTag);
                var update = Builders<Hashtag>.Update
                    .Inc(h => h.UsageCount, 1)
                    .Set(h => h.LastUsedAt, DateTime.UtcNow)
                    .Set(h => h.UpdatedAt, DateTime.UtcNow)
                    .SetOnInsert(h => h.Tag, tag)
                    .SetOnInsert(h => h.NormalizedTag, normalizedTag)
                    .SetOnInsert(h => h.CreatedAt, DateTime.UtcNow);

                await _hashtags.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating hashtag usage");
            return Result.Failure<bool>("Failed to update hashtag usage");
        }
    }

    public async Task<Result<List<Post>>> SearchPostsAsync(string searchTerm, int page = 1, int pageSize = 20)
    {
        try
        {
            var filter = Builders<Post>.Filter.And(
                Builders<Post>.Filter.Eq(p => p.IsDeleted, false),
                Builders<Post>.Filter.Eq(p => p.Visibility, PostVisibility.Public),
                Builders<Post>.Filter.Regex(p => p.Content, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"))
            );

            var posts = await _posts.Find(filter)
                .SortByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            return Result<List<Post>>.Success(posts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching posts with term {SearchTerm}", searchTerm);
            return Result.Failure<List<Post>>("Failed to search posts");
        }
    }
}
