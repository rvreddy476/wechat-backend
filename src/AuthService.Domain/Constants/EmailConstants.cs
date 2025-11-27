namespace AuthService.Domain.Constants;

public static class EmailConstants
{
    // Email Subjects
    public const string VerificationCodeSubject = "Verify Your WeChat Account";
    public const string PasswordResetSubject = "Reset Your WeChat Password";
    public const string WelcomeSubject = "Welcome to WeChat!";
    public const string AccountLockedSubject = "WeChat Account Locked";
    public const string SecurityAlertSubject = "WeChat Security Alert";

    // Email From
    public const string DefaultFromEmail = "noreply@wechat.com";
    public const string DefaultFromName = "WeChat";

    // Expiry Times
    public const int VerificationCodeExpiryMinutes = 10;
    public const int PasswordResetTokenExpiryMinutes = 60;
    public const int EmailVerificationTokenExpiryHours = 24;
}
