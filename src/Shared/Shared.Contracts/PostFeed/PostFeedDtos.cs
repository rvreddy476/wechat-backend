namespace Shared.Contracts.PostFeed;

/// <summary>
/// Post DTO
/// </summary>
public record PostDto
{
    public string Id { get; init; } = string.Empty;
    public string AuthorId { get; init; } = string.Empty;
    public string AuthorUsername { get; init; } = string.Empty;
    public string? AuthorAvatarUrl { get; init; }
    public string Content { get; init; } = string.Empty;
    public List<string> ImageUrls { get; init; } = new();
    public string? VideoUrl { get; init; }
    public int LikesCount { get; init; }
    public int CommentsCount { get; init; }
    public int SharesCount { get; init; }
    public bool IsLikedByCurrentUser { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public PostVisibility Visibility { get; init; }
}

/// <summary>
/// Comment DTO
/// </summary>
public record CommentDto
{
    public string Id { get; init; } = string.Empty;
    public string PostId { get; init; } = string.Empty;
    public string AuthorId { get; init; } = string.Empty;
    public string AuthorUsername { get; init; } = string.Empty;
    public string? AuthorAvatarUrl { get; init; }
    public string Content { get; init; } = string.Empty;
    public int LikesCount { get; init; }
    public bool IsLikedByCurrentUser { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? ParentCommentId { get; init; }
}

/// <summary>
/// Request to create a post
/// </summary>
public record CreatePostRequest
{
    public string Content { get; init; } = string.Empty;
    public List<string> ImageUrls { get; init; } = new();
    public string? VideoUrl { get; init; }
    public PostVisibility Visibility { get; init; } = PostVisibility.Public;
}

/// <summary>
/// Request to update a post
/// </summary>
public record UpdatePostRequest
{
    public string Content { get; init; } = string.Empty;
    public PostVisibility Visibility { get; init; }
}

/// <summary>
/// Request to create a comment
/// </summary>
public record CreateCommentRequest
{
    public string PostId { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string? ParentCommentId { get; init; }
}

/// <summary>
/// Request to get feed
/// </summary>
public record GetFeedRequest
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public FeedType Type { get; init; } = FeedType.Following;
}

/// <summary>
/// Post visibility enumeration
/// </summary>
public enum PostVisibility
{
    Public,
    Friends,
    Private
}

/// <summary>
/// Feed type enumeration
/// </summary>
public enum FeedType
{
    Following,
    Discover,
    MyPosts
}
