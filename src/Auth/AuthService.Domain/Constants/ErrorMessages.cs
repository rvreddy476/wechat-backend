namespace AuthService.Domain.Constants;

public static class ErrorMessages
{
    // Authentication Errors
    public const string InvalidCredentials = "Invalid username/email or password";
    public const string AccountLocked = "Account is locked due to multiple failed login attempts. Please try again later";
    public const string AccountInactive = "Account is inactive. Please contact support";
    public const string AccountDeleted = "Account has been deleted";
    public const string EmailNotVerified = "Email address has not been verified";

    // Registration Errors
    public const string EmailAlreadyExists = "An account with this email already exists";
    public const string PhoneNumberAlreadyExists = "An account with this phone number already exists";
    public const string UsernameAlreadyExists = "Username already exists";
    public const string HandlerAlreadyExists = "Handler already taken";
    public const string RegistrationFailed = "Registration failed. Please try again";

    // Verification Errors
    public const string VerificationCodeInvalid = "Invalid or expired verification code";
    public const string VerificationCodeExpired = "Verification code has expired. Please request a new one";
    public const string VerificationCodeAlreadyUsed = "Verification code has already been used";
    public const string VerificationCodeRateLimited = "Please wait {0} seconds before requesting a new code";
    public const string VerificationAlreadyCompleted = "Email/Phone is already verified";

    // Token Errors
    public const string RefreshTokenInvalid = "Invalid or expired refresh token";
    public const string RefreshTokenRevoked = "Refresh token has been revoked";
    public const string ResetTokenInvalid = "Invalid or expired reset token";
    public const string ResetTokenAlreadyUsed = "Reset token has already been used";

    // User Errors
    public const string UserNotFound = "User not found";
    public const string UserCreationFailed = "Failed to create user account";
    public const string UserUpdateFailed = "Failed to update user information";

    // General Errors
    public const string DatabaseError = "A database error occurred. Please try again later";
    public const string UnexpectedError = "An unexpected error occurred. Please try again later";
    public const string ServiceUnavailable = "Service is temporarily unavailable. Please try again later";
}
