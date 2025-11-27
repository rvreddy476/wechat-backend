using AuthService.Domain.Entities;

namespace AuthService.Domain.Interfaces;

public interface IAuthRepository
{
    // User Operations
    Task<User?> GetUserByIdAsync(Guid userId);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByUsernameAsync(string username);
    Task<User?> GetUserByPhoneNumberAsync(string phoneNumber);
    Task<Guid> CreateUserAsync(User user);
    Task<bool> UpdateUserAsync(User user);
    Task<bool> DeleteUserAsync(Guid userId);

    // Existence Checks
    Task<bool> EmailExistsAsync(string email);
    Task<bool> UsernameExistsAsync(string username);
    Task<bool> PhoneNumberExistsAsync(string phoneNumber);
    Task<bool> HandlerExistsAsync(string handler);

    // Verification Operations
    Task<bool> UpdateEmailVerificationStatusAsync(Guid userId, bool isVerified);
    Task<bool> UpdatePhoneVerificationStatusAsync(Guid userId, bool isVerified);

    // Login Management
    Task<bool> IncrementFailedLoginAttemptsAsync(Guid userId);
    Task<bool> ResetFailedLoginAttemptsAsync(Guid userId);
    Task<bool> UpdateLastLoginAsync(Guid userId);

    // Refresh Token Operations
    Task<Guid> CreateRefreshTokenAsync(RefreshToken token);
    Task<RefreshToken?> GetRefreshTokenAsync(string tokenHash);
    Task<bool> RevokeRefreshTokenAsync(Guid tokenId, string? reason = null);
    Task<bool> RevokeAllUserRefreshTokensAsync(Guid userId, string? reason = null);

    // Password Reset Operations
    Task<Guid> CreatePasswordResetTokenAsync(PasswordResetToken token);
    Task<PasswordResetToken?> GetPasswordResetTokenAsync(string tokenHash);
    Task<bool> MarkPasswordResetTokenAsUsedAsync(Guid tokenId);
    Task<bool> UpdatePasswordAsync(Guid userId, string newPasswordHash);
}
