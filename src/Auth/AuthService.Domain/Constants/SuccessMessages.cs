namespace AuthService.Domain.Constants;

public static class SuccessMessages
{
    // Registration
    public const string RegistrationSuccessful = "Registration successful. Verification codes sent to your email and phone";
    public const string RegistrationSuccessfulNoVerification = "Registration successful";

    // Login
    public const string LoginSuccessful = "Login successful";
    public const string LogoutSuccessful = "Logout successful";

    // Verification
    public const string VerificationCodeSent = "Verification code sent to {0}";
    public const string VerificationSuccessful = "Verification successful";
    public const string EmailVerified = "Email verified successfully";
    public const string PhoneVerified = "Phone number verified successfully";

    // Password
    public const string PasswordResetEmailSent = "Password reset email sent successfully";
    public const string PasswordResetSuccessful = "Password reset successful";
    public const string PasswordChanged = "Password changed successfully";

    // Token
    public const string TokenRefreshed = "Token refreshed successfully";
    public const string TokenRevoked = "Token revoked successfully";

    // User
    public const string ProfileUpdated = "Profile updated successfully";
    public const string AccountDeactivated = "Account deactivated successfully";
    public const string AccountReactivated = "Account reactivated successfully";
}
