namespace AuthService.Domain.Interfaces;

public interface IEmailService
{
    Task<bool> SendVerificationCodeAsync(string email, string code, string firstName);
    Task<bool> SendPasswordResetEmailAsync(string email, string resetToken, string firstName);
    Task<bool> SendWelcomeEmailAsync(string email, string firstName);
    Task<bool> SendAccountLockedEmailAsync(string email, string firstName, DateTime lockoutEnd);
}
