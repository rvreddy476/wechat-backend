namespace Shared.Domain.Constants;

/// <summary>
/// Shared constants used across all services
/// </summary>
public static class SharedConstants
{
    /// <summary>
    /// Verification code constants
    /// </summary>
    public static class VerificationCode
    {
        public const int Length = 6;
        public const int ExpirationMinutes = 10;
    }

    /// <summary>
    /// Pagination constants
    /// </summary>
    public static class Pagination
    {
        public const int DefaultPageSize = 20;
        public const int MaxPageSize = 100;
        public const int MinPageNumber = 1;
    }

    /// <summary>
    /// File upload constants
    /// </summary>
    public static class FileUpload
    {
        public const int MaxImageSizeMB = 10;
        public const int MaxVideoSizeMB = 100;
        public const int MaxFileSizeMB = 50;

        public static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        public static readonly string[] AllowedVideoExtensions = { ".mp4", ".mov", ".avi", ".mkv", ".webm" };
        public static readonly string[] AllowedDocumentExtensions = { ".pdf", ".doc", ".docx", ".txt" };
    }

    /// <summary>
    /// Chat and messaging constants
    /// </summary>
    public static class Chat
    {
        public const int MaxMessageLength = 5000;
        public const int MaxGroupNameLength = 100;
        public const int MaxGroupMembers = 500;
    }

    /// <summary>
    /// User profile constants
    /// </summary>
    public static class UserProfile
    {
        public const int MinUsernameLength = 3;
        public const int MaxUsernameLength = 50;
        public const int MaxDisplayNameLength = 100;
        public const int MaxBioLength = 500;
    }

    /// <summary>
    /// Post and feed constants
    /// </summary>
    public static class Post
    {
        public const int MaxContentLength = 10000;
        public const int MaxCommentLength = 1000;
        public const int MaxImagesPerPost = 9;
    }

    /// <summary>
    /// Cache key prefixes
    /// </summary>
    public static class CacheKeys
    {
        public const string UserPrefix = "user:";
        public const string SessionPrefix = "session:";
        public const string VerificationPrefix = "verification:";
        public const string RefreshTokenPrefix = "refresh:";
        public const string PostPrefix = "post:";
        public const string ChatPrefix = "chat:";
    }

    /// <summary>
    /// Role names
    /// </summary>
    public static class Roles
    {
        public const string User = "User";
        public const string Admin = "Admin";
        public const string Moderator = "Moderator";
    }
}
