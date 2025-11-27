namespace AuthService.Domain.Constants;

public static class AuthConstants
{
    // Password
    public const int MinPasswordLength = 8;
    public const int MaxPasswordLength = 128;
    public const int PasswordHashWorkFactor = 12; // BCrypt work factor

    // Username
    public const int MinUsernameLength = 3;
    public const int MaxUsernameLength = 30;

    // Name
    public const int MinNameLength = 2;
    public const int MaxFirstNameLength = 50;
    public const int MaxLastNameLength = 50;

    // Handler
    public const int MinHandlerLength = 3;
    public const int MaxHandlerLength = 30;

    // Email
    public const int MaxEmailLength = 255;

    // Verification
    public const int VerificationCodeLength = 6;
    public const int VerificationCodeRateLimitSeconds = 60;

    // Account Lockout
    public const int MaxFailedLoginAttempts = 5;
    public const int LockoutDurationMinutes = 30;

    // Age Restriction
    public const int MinimumAgeYears = 13;

    // Token
    public const int AccessTokenExpirationMinutes = 15;
    public const int RefreshTokenExpirationDays = 7;
}
