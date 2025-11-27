using AuthService.Api.Models;
using Shared.Domain.Common;

namespace AuthService.Api.Services;

public interface IVerificationService
{
    /// <summary>
    /// Generate a 6-digit verification code
    /// </summary>
    string GenerateCode();

    /// <summary>
    /// Send verification code to email
    /// </summary>
    Task<Result<VerificationCode>> SendEmailVerificationCodeAsync(Guid userId, string email, string firstName);

    /// <summary>
    /// Send verification code to phone
    /// </summary>
    Task<Result<VerificationCode>> SendPhoneVerificationCodeAsync(Guid userId, string phoneNumber);

    /// <summary>
    /// Verify a code
    /// </summary>
    Task<Result<bool>> VerifyCodeAsync(Guid userId, string code, VerificationType verificationType);

    /// <summary>
    /// Check if user can request a new verification code (rate limiting)
    /// </summary>
    Task<Result<bool>> CanRequestVerificationCodeAsync(Guid userId, VerificationType verificationType);

    /// <summary>
    /// Resend verification code
    /// </summary>
    Task<Result<VerificationCode>> ResendVerificationCodeAsync(Guid userId, VerificationType verificationType, string target, string? firstName = null);
}
