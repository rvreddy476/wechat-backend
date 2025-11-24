using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Shared.Domain.Common;

namespace UserProfileService.Api.Models;

public class UserProfile : Entity<string>, ISoftDelete
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public override string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.String)]
    public Guid UserId { get; set; }

    [BsonElement("username")]
    public string Username { get; set; } = string.Empty;

    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [BsonElement("bio")]
    public string? Bio { get; set; }

    [BsonElement("avatarUrl")]
    public string? AvatarUrl { get; set; }

    [BsonElement("coverImageUrl")]
    public string? CoverImageUrl { get; set; }

    [BsonElement("location")]
    public string? Location { get; set; }

    [BsonElement("website")]
    public string? Website { get; set; }

    [BsonElement("dateOfBirth")]
    public DateTime? DateOfBirth { get; set; }

    [BsonElement("isPrivate")]
    public bool IsPrivate { get; set; } = false;

    [BsonElement("isVerified")]
    public bool IsVerified { get; set; } = false;

    [BsonElement("stats")]
    public ProfileStats Stats { get; set; } = new();

    [BsonElement("socialLinks")]
    public SocialLinks SocialLinks { get; set; } = new();

    [BsonElement("preferences")]
    public ProfilePreferences Preferences { get; set; } = new();

    [BsonElement("createdAt")]
    public override DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public override DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("isDeleted")]
    public bool IsDeleted { get; set; } = false;

    [BsonElement("deletedAt")]
    public DateTime? DeletedAt { get; set; }
}

public class ProfileStats
{
    [BsonElement("followersCount")]
    public int FollowersCount { get; set; } = 0;

    [BsonElement("followingCount")]
    public int FollowingCount { get; set; } = 0;

    [BsonElement("postsCount")]
    public int PostsCount { get; set; } = 0;

    [BsonElement("videosCount")]
    public int VideosCount { get; set; } = 0;

    [BsonElement("totalViews")]
    public long TotalViews { get; set; } = 0;
}

public class SocialLinks
{
    [BsonElement("twitter")]
    public string? Twitter { get; set; }

    [BsonElement("instagram")]
    public string? Instagram { get; set; }

    [BsonElement("facebook")]
    public string? Facebook { get; set; }

    [BsonElement("youtube")]
    public string? YouTube { get; set; }

    [BsonElement("tiktok")]
    public string? TikTok { get; set; }
}

public class ProfilePreferences
{
    [BsonElement("showEmail")]
    public bool ShowEmail { get; set; } = false;

    [BsonElement("showDateOfBirth")]
    public bool ShowDateOfBirth { get; set; } = false;

    [BsonElement("showLocation")]
    public bool ShowLocation { get; set; } = true;

    [BsonElement("allowMessagesFromNonFollowers")]
    public bool AllowMessagesFromNonFollowers { get; set; } = true;

    [BsonElement("emailNotifications")]
    public bool EmailNotifications { get; set; } = true;

    [BsonElement("pushNotifications")]
    public bool PushNotifications { get; set; } = true;
}
