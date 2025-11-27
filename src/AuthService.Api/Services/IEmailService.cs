namespace AuthService.Api.Services;

public interface IEmailService
{
    /// <summary>
    /// Send a verification code via email
    /// </summary>
    Task<bool> SendVerificationCodeAsync(string email, string code, string firstName);

    /// <summary>
    /// Send a password reset email
    /// </summary>
    Task<bool> SendPasswordResetEmailAsync(string email, string resetToken, string firstName);

    /// <summary>
    /// Send a welcome email after registration
    /// </summary>
    Task<bool> SendWelcomeEmailAsync(string email, string firstName);
}
