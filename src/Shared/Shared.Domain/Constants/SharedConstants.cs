namespace Shared.Domain.Constants;

public static class SharedConstants
{
    public static class Pagination
    {
        public const int DefaultPageSize = 20;
        public const int MaxPageSize = 100;
    }

    public static class Jwt
    {
        public const int AccessTokenExpirationMinutes = 60;
        public const int RefreshTokenExpirationDays = 7;
    }

    public static class FileUpload
    {
        public const int MaxImageSizeMB = 10;
        public const int MaxVideoSizeMB = 100;
        public static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        public static readonly string[] AllowedVideoExtensions = { ".mp4", ".mov", ".avi" };
    }

    public static class CacheKeys
    {
        public const string UserPrefix = "user:";
        public const string SessionPrefix = "session:";
        public const string PostPrefix = "post:";
    }

    public static class Roles
    {
        public const string User = "User";
        public const string Admin = "Admin";
    }
}
