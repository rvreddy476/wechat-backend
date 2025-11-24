using AuthService.Api.Models;
using Shared.Domain.Common;

namespace AuthService.Api.Repositories;

public interface IAuthRepository
{
    Task<Result<UserDto>> RegisterUserAsync(string username, string email, string passwordHash, string? phoneNumber, string roleName = "User");
    Task<Result<UserAuthDto>> AuthenticateUserAsync(string emailOrUsername, string providedPassword);
    Task<Result<UserDto>> GetUserByIdAsync(Guid userId);
    Task<Result<bool>> UpdateUserProfileAsync(Guid userId, string? username, string? phoneNumber, string? bio, string? avatarUrl);
    Task<Result<bool>> ChangePasswordAsync(Guid userId, string currentPassword, string newPasswordHash);
    Task<Result<bool>> DeleteUserAsync(Guid userId);

    // Token Management
    Task<Result<Guid>> CreateRefreshTokenAsync(Guid userId, string tokenHash, DateTime expiresAt, string ipAddress, string userAgent);
    Task<Result<RefreshTokenDto>> GetRefreshTokenAsync(string tokenHash);
    Task<Result<bool>> RevokeRefreshTokenAsync(Guid tokenId, string reason);
    Task<Result<bool>> RevokeAllUserTokensAsync(Guid userId, string reason);

    // Email Verification
    Task<Result<Guid>> CreateEmailVerificationTokenAsync(Guid userId, string tokenHash, DateTime expiresAt);
    Task<Result<EmailVerificationTokenDto>> GetEmailVerificationTokenAsync(string tokenHash);
    Task<Result<bool>> MarkEmailAsVerifiedAsync(Guid userId);

    // Password Reset
    Task<Result<Guid>> CreatePasswordResetTokenAsync(Guid userId, string tokenHash, DateTime expiresAt);
    Task<Result<PasswordResetTokenDto>> GetPasswordResetTokenAsync(string tokenHash);
    Task<Result<bool>> ResetPasswordWithTokenAsync(Guid userId, string newPasswordHash);

    // Helper Methods
    Task<bool> EmailExistsAsync(string email);
    Task<bool> UsernameExistsAsync(string username);
}
