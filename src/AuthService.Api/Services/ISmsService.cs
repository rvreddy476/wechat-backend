namespace AuthService.Api.Services;

public interface ISmsService
{
    /// <summary>
    /// Send a verification code via SMS
    /// </summary>
    Task<bool> SendVerificationCodeAsync(string phoneNumber, string code);

    /// <summary>
    /// Send a password reset code via SMS
    /// </summary>
    Task<bool> SendPasswordResetCodeAsync(string phoneNumber, string code);
}
