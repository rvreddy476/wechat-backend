namespace AuthService.Domain.Interfaces;

public interface ISmsService
{
    Task<bool> SendVerificationCodeAsync(string phoneNumber, string code);
    Task<bool> SendPasswordResetCodeAsync(string phoneNumber, string code);
}
