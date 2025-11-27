using System.Data;
using Dapper;
using Npgsql;
using AuthService.Api.Models;
using Shared.Domain.Common;

namespace AuthService.Api.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly string _connectionString;
    private readonly ILogger<AuthRepository> _logger;

    public AuthRepository(IConfiguration configuration, ILogger<AuthRepository> logger)
    {
        _connectionString = configuration.GetConnectionString("PostgreSQL")
            ?? throw new InvalidOperationException("PostgreSQL connection string not found");
        _logger = logger;
    }

    private IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);

    public async Task<Result<UserDto>> RegisterUserAsync(
        string firstName,
        string lastName,
        string username,
        string email,
        string phoneNumber,
        string passwordHash,
        string? handler = null,
        string? gender = null,
        DateTime? dateOfBirth = null,
        string roleName = "User")
    {
        try
        {
            using var connection = CreateConnection();

            var parameters = new
            {
                p_first_name = firstName,
                p_last_name = lastName,
                p_username = username,
                p_email = email,
                p_password_hash = passwordHash,
                p_phone_number = phoneNumber,
                p_handler = handler,
                p_gender = gender,
                p_date_of_birth = dateOfBirth,
                p_ip_address = (string?)null,
                p_user_agent = (string?)null
            };

            var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "auth.sp_RegisterUser",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            if (result == null)
            {
                return Result<UserDto>.Failure("Registration failed");
            }

            var userDto = new UserDto
            {
                UserId = result.user_id,
                FirstName = result.first_name,
                LastName = result.last_name,
                Username = result.username,
                Email = result.email,
                PhoneNumber = result.phone_number,
                Handler = result.handler,
                IsEmailVerified = result.is_email_verified,
                IsPhoneVerified = result.is_phone_verified,
                IsActive = result.is_active,
                IsDeleted = result.is_deleted,
                Bio = result.bio,
                AvatarUrl = result.avatar_url,
                CreatedAt = result.created_at,
                UpdatedAt = result.updated_at,
                Roles = new List<string> { roleName }
            };

            return Result<UserDto>.Success(userDto);
        }
        catch (PostgresException ex) when (ex.SqlState == "P0001") // raise_exception
        {
            var errorMessage = ex.MessageText ?? "Registration failed";
            _logger.LogWarning("Registration failed: {Error}", errorMessage);
            return Result<UserDto>.Failure(errorMessage);
        }
        catch (PostgresException ex) when (ex.SqlState == "23505") // Unique violation
        {
            if (ex.ConstraintName?.Contains("email") == true)
                return Result<UserDto>.Failure(Errors.Authentication.DuplicateEmail);
            if (ex.ConstraintName?.Contains("username") == true)
                return Result<UserDto>.Failure(Errors.Authentication.DuplicateUsername);
            if (ex.ConstraintName?.Contains("phone") == true)
                return Result<UserDto>.Failure("Phone number already registered");
            if (ex.ConstraintName?.Contains("handler") == true)
                return Result<UserDto>.Failure("Handler already taken");

            return Result<UserDto>.Failure("Registration failed: Duplicate entry");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering user {Email}", email);
            return Result<UserDto>.Failure("An error occurred during registration");
        }
    }

    public async Task<Result<UserAuthDto>> AuthenticateUserAsync(string emailOrUsername, string providedPassword)
    {
        try
        {
            using var connection = CreateConnection();

            var parameters = new
            {
                p_email_or_username = emailOrUsername,
                p_provided_password = providedPassword
            };

            var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "auth.sp_AuthenticateUser",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            if (result == null)
            {
                return Result<UserAuthDto>.Failure(Errors.Authentication.InvalidCredentials);
            }

            // Get user roles
            var roles = await connection.QueryAsync<string>(
                "SELECT r.role_name FROM auth.roles r INNER JOIN auth.user_roles ur ON r.role_id = ur.role_id WHERE ur.user_id = @UserId",
                new { UserId = result.user_id }
            );

            var userAuthDto = new UserAuthDto
            {
                UserId = result.user_id,
                Username = result.username,
                Email = result.email,
                PasswordHash = result.password_hash,
                IsActive = result.is_active,
                IsDeleted = result.is_deleted,
                IsEmailVerified = result.is_email_verified,
                FailedLoginAttempts = result.failed_login_attempts,
                LockoutEnd = result.lockout_end,
                Roles = roles.ToList()
            };

            return Result<UserAuthDto>.Success(userAuthDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error authenticating user {EmailOrUsername}", emailOrUsername);
            return Result<UserAuthDto>.Failure(Errors.Authentication.InvalidCredentials);
        }
    }

    public async Task<Result<UserDto>> GetUserByIdAsync(Guid userId)
    {
        try
        {
            using var connection = CreateConnection();

            var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "auth.sp_GetUserById",
                new { p_user_id = userId },
                commandType: CommandType.StoredProcedure
            );

            if (result == null)
            {
                return Result<UserDto>.Failure(Errors.NotFound.User);
            }

            // Get user roles
            var roles = await connection.QueryAsync<string>(
                "SELECT r.role_name FROM auth.roles r INNER JOIN auth.user_roles ur ON r.role_id = ur.role_id WHERE ur.user_id = @UserId",
                new { UserId = userId }
            );

            var userDto = new UserDto
            {
                UserId = result.user_id,
                Username = result.username,
                Email = result.email,
                PhoneNumber = result.phone_number,
                IsEmailVerified = result.is_email_verified,
                IsPhoneVerified = result.is_phone_verified,
                IsActive = result.is_active,
                IsDeleted = result.is_deleted,
                Bio = result.bio,
                AvatarUrl = result.avatar_url,
                CreatedAt = result.created_at,
                UpdatedAt = result.updated_at,
                LastLoginAt = result.last_login_at,
                Roles = roles.ToList()
            };

            return Result<UserDto>.Success(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user {UserId}", userId);
            return Result<UserDto>.Failure("Failed to retrieve user");
        }
    }

    public async Task<Result<bool>> UpdateUserProfileAsync(Guid userId, string? username, string? phoneNumber, string? bio, string? avatarUrl)
    {
        try
        {
            using var connection = CreateConnection();

            var parameters = new
            {
                p_user_id = userId,
                p_username = username,
                p_phone_number = phoneNumber,
                p_bio = bio,
                p_avatar_url = avatarUrl
            };

            await connection.ExecuteAsync(
                "auth.sp_UpdateUserProfile",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return Result<bool>.Success(true);
        }
        catch (PostgresException ex) when (ex.SqlState == "23505")
        {
            return Result<bool>.Failure("Username or phone number already exists");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile {UserId}", userId);
            return Result<bool>.Failure("Failed to update profile");
        }
    }

    public async Task<Result<bool>> ChangePasswordAsync(Guid userId, string currentPassword, string newPasswordHash)
    {
        try
        {
            using var connection = CreateConnection();

            var parameters = new
            {
                p_user_id = userId,
                p_current_password = currentPassword,
                p_new_password_hash = newPasswordHash
            };

            await connection.ExecuteAsync(
                "auth.sp_ChangePassword",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return Result<bool>.Success(true);
        }
        catch (PostgresException ex) when (ex.Message.Contains("Invalid current password"))
        {
            return Result<bool>.Failure(Errors.Authentication.InvalidPassword);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user {UserId}", userId);
            return Result<bool>.Failure("Failed to change password");
        }
    }

    public async Task<Result<bool>> DeleteUserAsync(Guid userId)
    {
        try
        {
            using var connection = CreateConnection();

            await connection.ExecuteAsync(
                "auth.sp_DeleteUser",
                new { p_user_id = userId },
                commandType: CommandType.StoredProcedure
            );

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", userId);
            return Result<bool>.Failure("Failed to delete user");
        }
    }

    public async Task<Result<Guid>> CreateRefreshTokenAsync(Guid userId, string tokenHash, DateTime expiresAt, string ipAddress, string userAgent)
    {
        try
        {
            using var connection = CreateConnection();

            var parameters = new
            {
                p_user_id = userId,
                p_token_hash = tokenHash,
                p_expires_at = expiresAt,
                p_ip_address = ipAddress,
                p_user_agent = userAgent
            };

            var tokenId = await connection.QueryFirstAsync<Guid>(
                "auth.sp_CreateRefreshToken",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return Result<Guid>.Success(tokenId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating refresh token for user {UserId}", userId);
            return Result<Guid>.Failure("Failed to create refresh token");
        }
    }

    public async Task<Result<RefreshTokenDto>> GetRefreshTokenAsync(string tokenHash)
    {
        try
        {
            using var connection = CreateConnection();

            var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "auth.sp_ValidateRefreshToken",
                new { p_token_hash = tokenHash },
                commandType: CommandType.StoredProcedure
            );

            if (result == null)
            {
                return Result<RefreshTokenDto>.Failure(Errors.Authentication.InvalidToken);
            }

            var tokenDto = new RefreshTokenDto
            {
                TokenId = result.token_id,
                UserId = result.user_id,
                TokenHash = result.token_hash,
                ExpiresAt = result.expires_at,
                IsRevoked = result.is_revoked,
                CreatedAt = result.created_at
            };

            return Result<RefreshTokenDto>.Success(tokenDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating refresh token");
            return Result<RefreshTokenDto>.Failure(Errors.Authentication.InvalidToken);
        }
    }

    public async Task<Result<bool>> RevokeRefreshTokenAsync(Guid tokenId, string reason)
    {
        try
        {
            using var connection = CreateConnection();

            await connection.ExecuteAsync(
                "auth.sp_RevokeRefreshToken",
                new { p_token_id = tokenId, p_reason = reason },
                commandType: CommandType.StoredProcedure
            );

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking refresh token {TokenId}", tokenId);
            return Result<bool>.Failure("Failed to revoke token");
        }
    }

    public async Task<Result<bool>> RevokeAllUserTokensAsync(Guid userId, string reason)
    {
        try
        {
            using var connection = CreateConnection();

            await connection.ExecuteAsync(
                "auth.sp_RevokeAllUserTokens",
                new { p_user_id = userId, p_reason = reason },
                commandType: CommandType.StoredProcedure
            );

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking all tokens for user {UserId}", userId);
            return Result<bool>.Failure("Failed to revoke tokens");
        }
    }

    public async Task<Result<Guid>> CreateEmailVerificationTokenAsync(Guid userId, string tokenHash, DateTime expiresAt)
    {
        try
        {
            using var connection = CreateConnection();

            var sql = @"
                INSERT INTO auth.email_verification_tokens (user_id, token_hash, expires_at)
                VALUES (@UserId, @TokenHash, @ExpiresAt)
                RETURNING token_id";

            var tokenId = await connection.QueryFirstAsync<Guid>(sql, new
            {
                UserId = userId,
                TokenHash = tokenHash,
                ExpiresAt = expiresAt
            });

            return Result<Guid>.Success(tokenId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating email verification token for user {UserId}", userId);
            return Result<Guid>.Failure("Failed to create verification token");
        }
    }

    public async Task<Result<EmailVerificationTokenDto>> GetEmailVerificationTokenAsync(string tokenHash)
    {
        try
        {
            using var connection = CreateConnection();

            var sql = @"
                SELECT token_id, user_id, token_hash, expires_at, is_used
                FROM auth.email_verification_tokens
                WHERE token_hash = @TokenHash AND NOT is_used AND expires_at > NOW()";

            var result = await connection.QueryFirstOrDefaultAsync<EmailVerificationTokenDto>(sql, new { TokenHash = tokenHash });

            if (result == null)
            {
                return Result<EmailVerificationTokenDto>.Failure(Errors.Authentication.InvalidToken);
            }

            return Result<EmailVerificationTokenDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting email verification token");
            return Result<EmailVerificationTokenDto>.Failure("Failed to validate token");
        }
    }

    public async Task<Result<bool>> MarkEmailAsVerifiedAsync(Guid userId)
    {
        try
        {
            using var connection = CreateConnection();

            await connection.ExecuteAsync(
                "UPDATE auth.users SET is_email_verified = TRUE WHERE user_id = @UserId",
                new { UserId = userId }
            );

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking email as verified for user {UserId}", userId);
            return Result<bool>.Failure("Failed to verify email");
        }
    }

    public async Task<Result<Guid>> CreatePasswordResetTokenAsync(Guid userId, string tokenHash, DateTime expiresAt)
    {
        try
        {
            using var connection = CreateConnection();

            var sql = @"
                INSERT INTO auth.password_reset_tokens (user_id, token_hash, expires_at)
                VALUES (@UserId, @TokenHash, @ExpiresAt)
                RETURNING token_id";

            var tokenId = await connection.QueryFirstAsync<Guid>(sql, new
            {
                UserId = userId,
                TokenHash = tokenHash,
                ExpiresAt = expiresAt
            });

            return Result<Guid>.Success(tokenId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating password reset token for user {UserId}", userId);
            return Result<Guid>.Failure("Failed to create reset token");
        }
    }

    public async Task<Result<PasswordResetTokenDto>> GetPasswordResetTokenAsync(string tokenHash)
    {
        try
        {
            using var connection = CreateConnection();

            var sql = @"
                SELECT token_id, user_id, token_hash, expires_at, is_used
                FROM auth.password_reset_tokens
                WHERE token_hash = @TokenHash AND NOT is_used AND expires_at > NOW()";

            var result = await connection.QueryFirstOrDefaultAsync<PasswordResetTokenDto>(sql, new { TokenHash = tokenHash });

            if (result == null)
            {
                return Result<PasswordResetTokenDto>.Failure(Errors.Authentication.InvalidToken);
            }

            return Result<PasswordResetTokenDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting password reset token");
            return Result<PasswordResetTokenDto>.Failure("Failed to validate token");
        }
    }

    public async Task<Result<bool>> ResetPasswordWithTokenAsync(Guid userId, string newPasswordHash)
    {
        try
        {
            using var connection = CreateConnection();

            var sql = @"
                UPDATE auth.users
                SET password_hash = @NewPasswordHash,
                    security_stamp = gen_random_uuid(),
                    updated_at = NOW()
                WHERE user_id = @UserId";

            await connection.ExecuteAsync(sql, new
            {
                UserId = userId,
                NewPasswordHash = newPasswordHash
            });

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password for user {UserId}", userId);
            return Result<bool>.Failure("Failed to reset password");
        }
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        try
        {
            using var connection = CreateConnection();
            var count = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM auth.users WHERE email = @Email",
                new { Email = email }
            );
            return count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if email exists");
            return false;
        }
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        try
        {
            using var connection = CreateConnection();
            var count = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM auth.users WHERE username = @Username",
                new { Username = username }
            );
            return count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if username exists");
            return false;
        }
    }
}
