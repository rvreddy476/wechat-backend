using System.Data;
using AuthService.Domain.Constants;
using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using AuthService.Domain.Interfaces;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace AuthService.Infrastructure.Persistence;

public class VerificationRepository : IVerificationRepository
{
    private readonly string _connectionString;
    private readonly ILogger<VerificationRepository> _logger;

    public VerificationRepository(IConfiguration configuration, ILogger<VerificationRepository> logger)
    {
        _connectionString = configuration.GetConnectionString("AuthDb")
            ?? throw new InvalidOperationException("AuthDb connection string not found");
        _logger = logger;
    }

    private IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);

    public async Task<Guid> CreateVerificationCodeAsync(VerificationCode verificationCode)
    {
        try
        {
            using var connection = CreateConnection();

            var parameters = new
            {
                p_UserId = verificationCode.UserId,
                p_VerificationType = verificationCode.VerificationType.ToString(),
                p_Target = verificationCode.Target,
                p_Code = verificationCode.Code,
                p_ExpiryMinutes = EmailConstants.VerificationCodeExpiryMinutes
            };

            var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "auth.sp_CreateVerificationCode",
                parameters,
                commandType: CommandType.StoredProcedure);

            if (result == null)
            {
                return Guid.Empty;
            }

            return (Guid)result.verification_code_id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating verification code for user: {UserId}", verificationCode.UserId);
            return Guid.Empty;
        }
    }

    public async Task<VerificationCode?> GetVerificationCodeAsync(Guid userId, string code, VerificationType verificationType)
    {
        try
        {
            using var connection = CreateConnection();

            var sql = @"
                SELECT
                    verification_code_id AS VerificationCodeId,
                    user_id AS UserId,
                    code AS Code,
                    verification_type AS VerificationType,
                    target AS Target,
                    is_used AS IsUsed,
                    is_expired AS IsExpired,
                    created_at AS CreatedAt,
                    expires_at AS ExpiresAt,
                    used_at AS UsedAt
                FROM auth.verification_codes
                WHERE user_id = @UserId
                    AND code = @Code
                    AND verification_type = @VerificationType
                ORDER BY created_at DESC
                LIMIT 1";

            return await connection.QueryFirstOrDefaultAsync<VerificationCode>(sql, new
            {
                UserId = userId,
                Code = code,
                VerificationType = verificationType.ToString()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting verification code for user: {UserId}", userId);
            return null;
        }
    }

    public async Task<bool> MarkVerificationCodeAsUsedAsync(Guid verificationCodeId)
    {
        try
        {
            using var connection = CreateConnection();

            var sql = @"
                UPDATE auth.verification_codes
                SET
                    is_used = true,
                    used_at = @UsedAt
                WHERE verification_code_id = @VerificationCodeId";

            var rowsAffected = await connection.ExecuteAsync(sql, new
            {
                VerificationCodeId = verificationCodeId,
                UsedAt = DateTime.UtcNow
            });

            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking verification code as used: {VerificationCodeId}", verificationCodeId);
            return false;
        }
    }

    public async Task<bool> MarkVerificationCodeAsExpiredAsync(Guid verificationCodeId)
    {
        try
        {
            using var connection = CreateConnection();

            var sql = @"
                UPDATE auth.verification_codes
                SET is_expired = true
                WHERE verification_code_id = @VerificationCodeId";

            var rowsAffected = await connection.ExecuteAsync(sql, new { VerificationCodeId = verificationCodeId });
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking verification code as expired: {VerificationCodeId}", verificationCodeId);
            return false;
        }
    }

    public async Task<bool> CanRequestVerificationCodeAsync(Guid userId, VerificationType verificationType)
    {
        try
        {
            using var connection = CreateConnection();

            var parameters = new
            {
                p_UserId = userId,
                p_VerificationType = verificationType.ToString()
            };

            var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "auth.sp_CanRequestVerificationCode",
                parameters,
                commandType: CommandType.StoredProcedure);

            if (result == null)
            {
                return true;
            }

            return (bool)result.can_request;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user can request verification code: {UserId}", userId);
            return false;
        }
    }

    public async Task<int> GetSecondsUntilNextRequestAsync(Guid userId, VerificationType verificationType)
    {
        try
        {
            using var connection = CreateConnection();

            var sql = @"
                SELECT
                    GREATEST(0, EXTRACT(EPOCH FROM (
                        created_at + INTERVAL '60 seconds' - NOW()
                    ))::INTEGER) AS seconds_remaining
                FROM auth.verification_codes
                WHERE user_id = @UserId
                    AND verification_type = @VerificationType
                ORDER BY created_at DESC
                LIMIT 1";

            var seconds = await connection.ExecuteScalarAsync<int>(sql, new
            {
                UserId = userId,
                VerificationType = verificationType.ToString()
            });

            return seconds;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting seconds until next request for user: {UserId}", userId);
            return 0;
        }
    }

    public async Task<bool> InvalidateOldVerificationCodesAsync(Guid userId, VerificationType verificationType)
    {
        try
        {
            using var connection = CreateConnection();

            var sql = @"
                UPDATE auth.verification_codes
                SET is_expired = true
                WHERE user_id = @UserId
                    AND verification_type = @VerificationType
                    AND is_used = false
                    AND is_expired = false";

            await connection.ExecuteAsync(sql, new
            {
                UserId = userId,
                VerificationType = verificationType.ToString()
            });

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating old verification codes for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<int> CleanupExpiredVerificationCodesAsync()
    {
        try
        {
            using var connection = CreateConnection();

            var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "auth.sp_CleanupExpiredVerificationCodes",
                commandType: CommandType.StoredProcedure);

            if (result == null)
            {
                return 0;
            }

            return (int)result.deleted_count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up expired verification codes");
            return 0;
        }
    }
}
