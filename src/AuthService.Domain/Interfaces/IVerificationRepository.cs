using AuthService.Domain.Entities;
using AuthService.Domain.Enums;

namespace AuthService.Domain.Interfaces;

public interface IVerificationRepository
{
    Task<Guid> CreateVerificationCodeAsync(VerificationCode verificationCode);
    Task<VerificationCode?> GetVerificationCodeAsync(Guid userId, string code, VerificationType verificationType);
    Task<bool> MarkVerificationCodeAsUsedAsync(Guid verificationCodeId);
    Task<bool> MarkVerificationCodeAsExpiredAsync(Guid verificationCodeId);
    Task<bool> CanRequestVerificationCodeAsync(Guid userId, VerificationType verificationType);
    Task<int> GetSecondsUntilNextRequestAsync(Guid userId, VerificationType verificationType);
    Task<bool> InvalidateOldVerificationCodesAsync(Guid userId, VerificationType verificationType);
    Task<int> CleanupExpiredVerificationCodesAsync();
}
