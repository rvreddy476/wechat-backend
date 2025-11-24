namespace Shared.Contracts.UserProfile;

public record UserProfileDto
{
    public required string UserId { get; init; }
    public required string Username { get; init; }
    public required string DisplayName { get; init; }
    public string? Bio { get; init; }
    public string? AvatarUrl { get; init; }
    public string? BannerUrl { get; init; }
    public string? Location { get; init; }
    public string? Website { get; init; }
    public bool Verified { get; init; }
    public string? VerifiedType { get; init; }
    public bool IsPrivate { get; init; }
    public UserStatsDto Stats { get; init; } = new();
    public DateTime CreatedAt { get; init; }
    public DateTime? LastActiveAt { get; init; }
}

public record UserStatsDto
{
    public int FollowersCount { get; init; }
    public int FollowingCount { get; init; }
    public int PostsCount { get; init; }
    public int VideosCount { get; init; }
    public int ShortsCount { get; init; }
    public int LikesReceived { get; init; }
    public int ViewsReceived { get; init; }
}

public record ProfileDto
{
    public Guid UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string? Bio { get; init; }
    public string? AvatarUrl { get; init; }
    public string? CoverImageUrl { get; init; }
    public string? Location { get; init; }
    public string? Website { get; init; }
    public DateTime? DateOfBirth { get; init; }
    public bool IsPrivate { get; init; }
    public bool IsVerified { get; init; }
    public ProfileStatsDto Stats { get; init; } = new();
    public SocialLinksDto SocialLinks { get; init; } = new();
    public DateTime CreatedAt { get; init; }
}

public record ProfileStatsDto
{
    public int FollowersCount { get; init; }
    public int FollowingCount { get; init; }
    public int PostsCount { get; init; }
    public int VideosCount { get; init; }
    public long TotalViews { get; init; }
}

public record SocialLinksDto
{
    public string? Twitter { get; init; }
    public string? Instagram { get; init; }
    public string? Facebook { get; init; }
    public string? YouTube { get; init; }
    public string? TikTok { get; init; }
}

public record ProfilePreferencesDto
{
    public bool? ShowEmail { get; init; }
    public bool? ShowDateOfBirth { get; init; }
    public bool? ShowLocation { get; init; }
    public bool? AllowMessagesFromNonFollowers { get; init; }
    public bool? EmailNotifications { get; init; }
    public bool? PushNotifications { get; init; }
}

public record UpdateProfileRequest
{
    public string? DisplayName { get; init; }
    public string? Bio { get; init; }
    public string? AvatarUrl { get; init; }
    public string? CoverImageUrl { get; init; }
    public string? Location { get; init; }
    public string? Website { get; init; }
    public DateTime? DateOfBirth { get; init; }
    public bool? IsPrivate { get; init; }
    public SocialLinksDto? SocialLinks { get; init; }
    public ProfilePreferencesDto? Preferences { get; init; }
}

public record BlockUserRequest
{
    public string? Reason { get; init; }
}

public record FollowUserRequest
{
    public required string UserIdToFollow { get; init; }
}

public record FollowResponseDto
{
    public required string FollowerId { get; init; }
    public required string FollowingId { get; init; }
    public bool IsAccepted { get; init; }
    public DateTime FollowedAt { get; init; }
}

public record FollowerDto
{
    public required string UserId { get; init; }
    public required string Username { get; init; }
    public required string DisplayName { get; init; }
    public string? AvatarUrl { get; init; }
    public bool Verified { get; init; }
    public DateTime FollowedAt { get; init; }
}
