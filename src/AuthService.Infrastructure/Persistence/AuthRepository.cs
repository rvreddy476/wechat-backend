using System.Data;
using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using AuthService.Domain.Interfaces;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace AuthService.Infrastructure.Persistence;

public class AuthRepository : IAuthRepository
{
    private readonly string _connectionString;
    private readonly ILogger<AuthRepository> _logger;

    public AuthRepository(IConfiguration configuration, ILogger<AuthRepository> logger)
    {
        _connectionString = configuration.GetConnectionString("AuthDb")
            ?? throw new InvalidOperationException("AuthDb connection string not found");
        _logger = logger;
    }

    private IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        try
        {
            using var connection = CreateConnection();

            var sql = @"
                SELECT
                    u.user_id AS UserId,
                    u.first_name AS FirstName,
                    u.last_name AS LastName,
                    u.username AS Username,
                    u.email AS Email,
                    u.password_hash AS PasswordHash,
                    u.phone_number AS PhoneNumber,
                    u.handler AS Handler,
                    u.gender AS Gender,
                    u.date_of_birth AS DateOfBirth,
                    u.is_email_verified AS IsEmailVerified,
                    u.is_phone_verified AS IsPhoneVerified,
                    u.is_active AS IsActive,
                    u.is_deleted AS IsDeleted,
                    u.bio AS Bio,
                    u.avatar_url AS AvatarUrl,
                    u.failed_login_attempts AS FailedLoginAttempts,
                    u.lockout_end AS LockoutEnd,
                    u.created_at AS CreatedAt,
                    u.updated_at AS UpdatedAt,
                    u.last_login_at AS LastLoginAt
                FROM auth.users u
                WHERE u.user_id = @UserId";

            var user = await connection.QueryFirstOrDefaultAsync<User>(sql, new { UserId = userId });

            if (user != null)
            {
                // Get user roles
                user.Roles = await GetUserRolesAsync(connection, userId);
            }

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by ID: {UserId}", userId);
            return null;
        }
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        try
        {
            using var connection = CreateConnection();

            var sql = @"
                SELECT
                    u.user_id AS UserId,
                    u.first_name AS FirstName,
                    u.last_name AS LastName,
                    u.username AS Username,
                    u.email AS Email,
                    u.password_hash AS PasswordHash,
                    u.phone_number AS PhoneNumber,
                    u.handler AS Handler,
                    u.gender AS Gender,
                    u.date_of_birth AS DateOfBirth,
                    u.is_email_verified AS IsEmailVerified,
                    u.is_phone_verified AS IsPhoneVerified,
                    u.is_active AS IsActive,
                    u.is_deleted AS IsDeleted,
                    u.bio AS Bio,
                    u.avatar_url AS AvatarUrl,
                    u.failed_login_attempts AS FailedLoginAttempts,
                    u.lockout_end AS LockoutEnd,
                    u.created_at AS CreatedAt,
                    u.updated_at AS UpdatedAt,
                    u.last_login_at AS LastLoginAt
                FROM auth.users u
                WHERE LOWER(u.email) = LOWER(@Email)";

            var user = await connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });

            if (user != null)
            {
                user.Roles = await GetUserRolesAsync(connection, user.UserId);
            }

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by email: {Email}", email);
            return null;
        }
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        try
        {
            using var connection = CreateConnection();

            var sql = @"
                SELECT
                    u.user_id AS UserId,
                    u.first_name AS FirstName,
                    u.last_name AS LastName,
                    u.username AS Username,
                    u.email AS Email,
                    u.password_hash AS PasswordHash,
                    u.phone_number AS PhoneNumber,
                    u.handler AS Handler,
                    u.gender AS Gender,
                    u.date_of_birth AS DateOfBirth,
                    u.is_email_verified AS IsEmailVerified,
                    u.is_phone_verified AS IsPhoneVerified,
                    u.is_active AS IsActive,
                    u.is_deleted AS IsDeleted,
                    u.bio AS Bio,
                    u.avatar_url AS AvatarUrl,
                    u.failed_login_attempts AS FailedLoginAttempts,
                    u.lockout_end AS LockoutEnd,
                    u.created_at AS CreatedAt,
                    u.updated_at AS UpdatedAt,
                    u.last_login_at AS LastLoginAt
                FROM auth.users u
                WHERE LOWER(u.username) = LOWER(@Username)";

            var user = await connection.QueryFirstOrDefaultAsync<User>(sql, new { Username = username });

            if (user != null)
            {
                user.Roles = await GetUserRolesAsync(connection, user.UserId);
            }

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by username: {Username}", username);
            return null;
        }
    }

    public async Task<User?> GetUserByPhoneNumberAsync(string phoneNumber)
    {
        try
        {
            using var connection = CreateConnection();

            var sql = @"
                SELECT
                    u.user_id AS UserId,
                    u.first_name AS FirstName,
                    u.last_name AS LastName,
                    u.username AS Username,
                    u.email AS Email,
                    u.password_hash AS PasswordHash,
                    u.phone_number AS PhoneNumber,
                    u.handler AS Handler,
                    u.gender AS Gender,
                    u.date_of_birth AS DateOfBirth,
                    u.is_email_verified AS IsEmailVerified,
                    u.is_phone_verified AS IsPhoneVerified,
                    u.is_active AS IsActive,
                    u.is_deleted AS IsDeleted,
                    u.bio AS Bio,
                    u.avatar_url AS AvatarUrl,
                    u.failed_login_attempts AS FailedLoginAttempts,
                    u.lockout_end AS LockoutEnd,
                    u.created_at AS CreatedAt,
                    u.updated_at AS UpdatedAt,
                    u.last_login_at AS LastLoginAt
                FROM auth.users u
                WHERE u.phone_number = @PhoneNumber";

            var user = await connection.QueryFirstOrDefaultAsync<User>(sql, new { PhoneNumber = phoneNumber });

            if (user != null)
            {
                user.Roles = await GetUserRolesAsync(connection, user.UserId);
            }

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by phone number: {PhoneNumber}", phoneNumber);
            return null;
        }
    }

    public async Task<Guid> CreateUserAsync(User user)
    {
        try
        {
            using var connection = CreateConnection();

            var parameters = new
            {
                p_first_name = user.FirstName,
                p_last_name = user.LastName,
                p_username = user.Username,
                p_email = user.Email,
                p_password_hash = user.PasswordHash,
                p_phone_number = user.PhoneNumber,
                p_handler = user.Handler,
                p_gender = user.Gender.ToString(),
                p_date_of_birth = user.DateOfBirth,
                p_ip_address = (string?)null,
                p_user_agent = (string?)null
            };

            var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "auth.sp_RegisterUser",
                parameters,
                commandType: CommandType.StoredProcedure);

            if (result == null)
            {
                return Guid.Empty;
            }

            return (Guid)result.user_id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user: {Email}", user.Email);
            return Guid.Empty;
        }
    }

    public async Task<bool> UpdateUserAsync(User user)
    {
        try
        {
            using var connection = CreateConnection();

            var sql = @"
                UPDATE auth.users
                SET
                    first_name = @FirstName,
                    last_name = @LastName,
                    phone_number = @PhoneNumber,
                    handler = @Handler,
                    gender = @Gender,
                    date_of_birth = @DateOfBirth,
                    bio = @Bio,
                    avatar_url = @AvatarUrl,
                    updated_at = @UpdatedAt
                WHERE user_id = @UserId";

            var rowsAffected = await connection.ExecuteAsync(sql, new
            {
                user.UserId,
                user.FirstName,
                user.LastName,
                user.PhoneNumber,
                user.Handler,
                Gender = user.Gender.ToString(),
                user.DateOfBirth,
                user.Bio,
                user.AvatarUrl,
                UpdatedAt = DateTime.UtcNow
            });

            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId}", user.UserId);
            return false;
        }
    }

    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        try
        {
            using var connection = CreateConnection();

            var sql = @"
                UPDATE auth.users
                SET
                    is_deleted = true,
                    is_active = false,
                    updated_at = @UpdatedAt
                WHERE user_id = @UserId";

            var rowsAffected = await connection.ExecuteAsync(sql, new
            {
                UserId = userId,
                UpdatedAt = DateTime.UtcNow
            });

            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user: {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        try
        {
            using var connection = CreateConnection();

            var sql = "SELECT EXISTS(SELECT 1 FROM auth.users WHERE LOWER(email) = LOWER(@Email))";
            return await connection.ExecuteScalarAsync<bool>(sql, new { Email = email });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if email exists: {Email}", email);
            return false;
        }
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        try
        {
            using var connection = CreateConnection();

            var sql = "SELECT EXISTS(SELECT 1 FROM auth.users WHERE LOWER(username) = LOWER(@Username))";
            return await connection.ExecuteScalarAsync<bool>(sql, new { Username = username });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if username exists: {Username}", username);
            return false;
        }
    }

    public async Task<bool> PhoneNumberExistsAsync(string phoneNumber)
    {
        try
        {
            using var connection = CreateConnection();

            var sql = "SELECT EXISTS(SELECT 1 FROM auth.users WHERE phone_number = @PhoneNumber)";
            return await connection.ExecuteScalarAsync<bool>(sql, new { PhoneNumber = phoneNumber });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if phone number exists: {PhoneNumber}", phoneNumber);
            return false;
        }
    }

    public async Task<bool> HandlerExistsAsync(string handler)
    {
        try
        {
            using var connection = CreateConnection();

            var sql = "SELECT EXISTS(SELECT 1 FROM auth.users WHERE LOWER(handler) = LOWER(@Handler))";
            return await connection.ExecuteScalarAsync<bool>(sql, new { Handler = handler });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if handler exists: {Handler}", handler);
            return false;
        }
    }

    public async Task<bool> UpdateEmailVerificationStatusAsync(Guid userId, bool isVerified)
    {
        try
        {
            using var connection = CreateConnection();

            var sql = @"
                UPDATE auth.users
                SET
                    is_email_verified = @IsVerified,
                    updated_at = @UpdatedAt
                WHERE user_id = @UserId";

            var rowsAffected = await connection.ExecuteAsync(sql, new
            {
                UserId = userId,
                IsVerified = isVerified,
                UpdatedAt = DateTime.UtcNow
            });

            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating email verification status for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> UpdatePhoneVerificationStatusAsync(Guid userId, bool isVerified)
    {
        try
        {
            using var connection = CreateConnection();

            var sql = @"
                UPDATE auth.users
                SET
                    is_phone_verified = @IsVerified,
                    updated_at = @UpdatedAt
                WHERE user_id = @UserId";

            var rowsAffected = await connection.ExecuteAsync(sql, new
            {
                UserId = userId,
                IsVerified = isVerified,
                UpdatedAt = DateTime.UtcNow
            });

            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating phone verification status for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> IncrementFailedLoginAttemptsAsync(Guid userId)
    {
        try
        {
            using var connection = CreateConnection();

            var sql = @"
                UPDATE auth.users
                SET
                    failed_login_attempts = failed_login_attempts + 1,
                    lockout_end = CASE
                        WHEN failed_login_attempts + 1 >= 5
                        THEN NOW() + INTERVAL '30 minutes'
                        ELSE lockout_end
                    END,
                    updated_at = @UpdatedAt
                WHERE user_id = @UserId";

            var rowsAffected = await connection.ExecuteAsync(sql, new
            {
                UserId = userId,
                UpdatedAt = DateTime.UtcNow
            });

            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing failed login attempts for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> ResetFailedLoginAttemptsAsync(Guid userId)
    {
        try
        {
            using var connection = CreateConnection();

            var sql = @"
                UPDATE auth.users
                SET
                    failed_login_attempts = 0,
                    lockout_end = NULL,
                    updated_at = @UpdatedAt
                WHERE user_id = @UserId";

            var rowsAffected = await connection.ExecuteAsync(sql, new
            {
                UserId = userId,
                UpdatedAt = DateTime.UtcNow
            });

            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting failed login attempts for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> UpdateLastLoginAsync(Guid userId)
    {
        try
        {
            using var connection = CreateConnection();

            var sql = @"
                UPDATE auth.users
                SET
                    last_login_at = @LastLoginAt,
                    updated_at = @UpdatedAt
                WHERE user_id = @UserId";

            var rowsAffected = await connection.ExecuteAsync(sql, new
            {
                UserId = userId,
                LastLoginAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating last login for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<Guid> CreateRefreshTokenAsync(RefreshToken token)
    {
        try
        {
            using var connection = CreateConnection();

            var sql = @"
                INSERT INTO auth.refresh_tokens (
                    token_id,
                    user_id,
                    token_hash,
                    expires_at,
                    is_revoked,
                    created_at
                )
                VALUES (
                    @TokenId,
                    @UserId,
                    @TokenHash,
                    @ExpiresAt,
                    @IsRevoked,
                    @CreatedAt
                )
                RETURNING token_id";

            var tokenId = await connection.ExecuteScalarAsync<Guid>(sql, token);
            return tokenId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating refresh token for user: {UserId}", token.UserId);
            return Guid.Empty;
        }
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(string tokenHash)
    {
        try
        {
            using var connection = CreateConnection();

            var sql = @"
                SELECT
                    token_id AS TokenId,
                    user_id AS UserId,
                    token_hash AS TokenHash,
                    expires_at AS ExpiresAt,
                    is_revoked AS IsRevoked,
                    created_at AS CreatedAt,
                    revoked_reason AS RevokedReason,
                    revoked_at AS RevokedAt
                FROM auth.refresh_tokens
                WHERE token_hash = @TokenHash";

            return await connection.QueryFirstOrDefaultAsync<RefreshToken>(sql, new { TokenHash = tokenHash });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting refresh token");
            return null;
        }
    }

    public async Task<bool> RevokeRefreshTokenAsync(Guid tokenId, string? reason = null)
    {
        try
        {
            using var connection = CreateConnection();

            var sql = @"
                UPDATE auth.refresh_tokens
                SET
                    is_revoked = true,
                    revoked_reason = @Reason,
                    revoked_at = @RevokedAt
                WHERE token_id = @TokenId";

            var rowsAffected = await connection.ExecuteAsync(sql, new
            {
                TokenId = tokenId,
                Reason = reason,
                RevokedAt = DateTime.UtcNow
            });

            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking refresh token: {TokenId}", tokenId);
            return false;
        }
    }

    public async Task<bool> RevokeAllUserRefreshTokensAsync(Guid userId, string? reason = null)
    {
        try
        {
            using var connection = CreateConnection();

            var sql = @"
                UPDATE auth.refresh_tokens
                SET
                    is_revoked = true,
                    revoked_reason = @Reason,
                    revoked_at = @RevokedAt
                WHERE user_id = @UserId AND is_revoked = false";

            var rowsAffected = await connection.ExecuteAsync(sql, new
            {
                UserId = userId,
                Reason = reason,
                RevokedAt = DateTime.UtcNow
            });

            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking all refresh tokens for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<Guid> CreatePasswordResetTokenAsync(PasswordResetToken token)
    {
        try
        {
            using var connection = CreateConnection();

            var sql = @"
                INSERT INTO auth.password_reset_tokens (
                    token_id,
                    user_id,
                    token_hash,
                    expires_at,
                    is_used,
                    created_at
                )
                VALUES (
                    @TokenId,
                    @UserId,
                    @TokenHash,
                    @ExpiresAt,
                    @IsUsed,
                    @CreatedAt
                )
                RETURNING token_id";

            var tokenId = await connection.ExecuteScalarAsync<Guid>(sql, token);
            return tokenId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating password reset token for user: {UserId}", token.UserId);
            return Guid.Empty;
        }
    }

    public async Task<PasswordResetToken?> GetPasswordResetTokenAsync(string tokenHash)
    {
        try
        {
            using var connection = CreateConnection();

            var sql = @"
                SELECT
                    token_id AS TokenId,
                    user_id AS UserId,
                    token_hash AS TokenHash,
                    expires_at AS ExpiresAt,
                    is_used AS IsUsed,
                    created_at AS CreatedAt,
                    used_at AS UsedAt
                FROM auth.password_reset_tokens
                WHERE token_hash = @TokenHash";

            return await connection.QueryFirstOrDefaultAsync<PasswordResetToken>(sql, new { TokenHash = tokenHash });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting password reset token");
            return null;
        }
    }

    public async Task<bool> MarkPasswordResetTokenAsUsedAsync(Guid tokenId)
    {
        try
        {
            using var connection = CreateConnection();

            var sql = @"
                UPDATE auth.password_reset_tokens
                SET
                    is_used = true,
                    used_at = @UsedAt
                WHERE token_id = @TokenId";

            var rowsAffected = await connection.ExecuteAsync(sql, new
            {
                TokenId = tokenId,
                UsedAt = DateTime.UtcNow
            });

            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking password reset token as used: {TokenId}", tokenId);
            return false;
        }
    }

    public async Task<bool> UpdatePasswordAsync(Guid userId, string newPasswordHash)
    {
        try
        {
            using var connection = CreateConnection();

            var sql = @"
                UPDATE auth.users
                SET
                    password_hash = @PasswordHash,
                    updated_at = @UpdatedAt
                WHERE user_id = @UserId";

            var rowsAffected = await connection.ExecuteAsync(sql, new
            {
                UserId = userId,
                PasswordHash = newPasswordHash,
                UpdatedAt = DateTime.UtcNow
            });

            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating password for user: {UserId}", userId);
            return false;
        }
    }

    private static async Task<List<string>> GetUserRolesAsync(IDbConnection connection, Guid userId)
    {
        var sql = @"
            SELECT r.role_name
            FROM auth.user_roles ur
            INNER JOIN auth.roles r ON ur.role_id = r.role_id
            WHERE ur.user_id = @UserId";

        var roles = await connection.QueryAsync<string>(sql, new { UserId = userId });
        return roles.ToList();
    }
}
