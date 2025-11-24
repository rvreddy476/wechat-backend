using PostFeedService.Api.Models;
using Shared.Domain.Common;

namespace PostFeedService.Api.Repositories;

public interface IPostRepository
{
    // Post Management
    Task<Result<Post>> CreatePostAsync(Post post);
    Task<Result<Post>> GetPostByIdAsync(string postId);
    Task<Result<bool>> UpdatePostAsync(string postId, Post post);
    Task<Result<bool>> DeletePostAsync(string postId, Guid userId);
    Task<Result<List<Post>>> GetUserPostsAsync(Guid userId, int page = 1, int pageSize = 20);

    // Feed
    Task<Result<List<Post>>> GetTimelineFeedAsync(Guid userId, List<Guid> followingIds, int page = 1, int pageSize = 20);
    Task<Result<List<Post>>> GetExploreFeedAsync(int page = 1, int pageSize = 20);
    Task<Result<List<Post>>> GetTrendingPostsAsync(int limit = 10);

    // Interactions
    Task<Result<bool>> IncrementViewCountAsync(string postId);
    Task<Result<bool>> UpdatePostStatsAsync(string postId, string statField, int incrementBy);

    // Comments
    Task<Result<Comment>> CreateCommentAsync(Comment comment);
    Task<Result<Comment>> GetCommentByIdAsync(string commentId);
    Task<Result<bool>> UpdateCommentAsync(string commentId, Comment comment);
    Task<Result<bool>> DeleteCommentAsync(string commentId, Guid userId);
    Task<Result<List<Comment>>> GetPostCommentsAsync(string postId, int page = 1, int pageSize = 20);
    Task<Result<List<Comment>>> GetCommentRepliesAsync(string parentCommentId, int page = 1, int pageSize = 10);

    // Reactions
    Task<Result<Reaction>> AddReactionAsync(Reaction reaction);
    Task<Result<bool>> RemoveReactionAsync(string targetId, Guid userId, ReactionTargetType targetType);
    Task<Result<Reaction>> GetUserReactionAsync(string targetId, Guid userId, ReactionTargetType targetType);
    Task<Result<List<Reaction>>> GetReactionsAsync(string targetId, ReactionTargetType targetType, int page = 1, int pageSize = 50);
    Task<Result<Dictionary<ReactionType, int>>> GetReactionCountsAsync(string targetId, ReactionTargetType targetType);

    // Hashtags
    Task<Result<List<Post>>> GetPostsByHashtagAsync(string hashtag, int page = 1, int pageSize = 20);
    Task<Result<List<Hashtag>>> GetTrendingHashtagsAsync(int limit = 10);
    Task<Result<bool>> UpdateHashtagUsageAsync(List<string> hashtags);

    // Search
    Task<Result<List<Post>>> SearchPostsAsync(string searchTerm, int page = 1, int pageSize = 20);
}
