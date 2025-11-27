using System.Data;
using System.Security.Cryptography;
using AuthService.Api.Models;
using Dapper;
using Npgsql;
using Shared.Domain.Common;

namespace AuthService.Api.Services;

public class VerificationService : IVerificationService
{
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;
    private readonly ILogger<VerificationService> _logger;

    public VerificationService(
        IConfiguration configuration,
        IEmailService emailService,
        ISmsService smsService,
        ILogger<VerificationService> logger)
    {
        _configuration = configuration;
        _emailService = emailService;
        _smsService = smsService;
        _logger = logger;
    }

    private NpgsqlConnection CreateConnection()
    {
        return new NpgsqlConnection(_configuration.GetConnectionString("AuthDb"));
    }

    public string GenerateCode()
    {
        // Generate a cryptographically secure 6-digit code
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[4];
        rng.GetBytes(bytes);

        // Convert to int and ensure it's 6 digits
        var randomNumber = BitConverter.ToUInt32(bytes, 0);
        var code = (randomNumber % 1000000).ToString("D6");

        return code;
    }

    public async Task<Result<VerificationCode>> SendEmailVerificationCodeAsync(
        Guid userId,
        string email,
        string firstName)
    {
        try
        {
            // Check rate limiting
            var canRequest = await CanRequestVerificationCodeAsync(userId, VerificationType.Email);
            if (!canRequest.IsSuccess)
            {
                return Result<VerificationCode>.Failure(canRequest.Error);
            }

            // Generate code
            var code = GenerateCode();

            // Store in database
            using var connection = CreateConnection();
            var parameters = new
            {
                p_UserId = userId,
                p_VerificationType = "Email",
                p_Target = email,
                p_Code = code,
                p_ExpiryMinutes = 10
            };

            var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "auth.sp_CreateVerificationCode",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            if (result == null)
            {
                return Result<VerificationCode>.Failure("Failed to create verification code");
            }

            var verificationCode = new VerificationCode
            {
                VerificationCodeId = result.verification_code_id,
                UserId = userId,
                Code = result.code,
                VerificationType = VerificationType.Email,
                Target = result.target,
                ExpiresAt = result.expires_at,
                CreatedAt = result.created_at
            };

            // Send email
            var emailSent = await _emailService.SendVerificationCodeAsync(email, code, firstName);
            if (!emailSent)
            {
                _logger.LogWarning("Failed to send verification email to {Email}, but code was created", email);
                // Don't fail the request - code is still valid
            }

            return Result<VerificationCode>.Success(verificationCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email verification code for user {UserId}", userId);
            return Result<VerificationCode>.Failure("An error occurred while sending verification code");
        }
    }

    public async Task<Result<VerificationCode>> SendPhoneVerificationCodeAsync(
        Guid userId,
        string phoneNumber)
    {
        try
        {
            // Check rate limiting
            var canRequest = await CanRequestVerificationCodeAsync(userId, VerificationType.Phone);
            if (!canRequest.IsSuccess)
            {
                return Result<VerificationCode>.Failure(canRequest.Error);
            }

            // Generate code
            var code = GenerateCode();

            // Store in database
            using var connection = CreateConnection();
            var parameters = new
            {
                p_UserId = userId,
                p_VerificationType = "Phone",
                p_Target = phoneNumber,
                p_Code = code,
                p_ExpiryMinutes = 10
            };

            var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "auth.sp_CreateVerificationCode",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            if (result == null)
            {
                return Result<VerificationCode>.Failure("Failed to create verification code");
            }

            var verificationCode = new VerificationCode
            {
                VerificationCodeId = result.verification_code_id,
                UserId = userId,
                Code = result.code,
                VerificationType = VerificationType.Phone,
                Target = result.target,
                ExpiresAt = result.expires_at,
                CreatedAt = result.created_at
            };

            // Send SMS
            var smsSent = await _smsService.SendVerificationCodeAsync(phoneNumber, code);
            if (!smsSent)
            {
                _logger.LogWarning("Failed to send verification SMS to {PhoneNumber}, but code was created", phoneNumber);
                // Don't fail the request - code is still valid
            }

            return Result<VerificationCode>.Success(verificationCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending phone verification code for user {UserId}", userId);
            return Result<VerificationCode>.Failure("An error occurred while sending verification code");
        }
    }

    public async Task<Result<bool>> VerifyCodeAsync(
        Guid userId,
        string code,
        VerificationType verificationType)
    {
        try
        {
            using var connection = CreateConnection();
            var parameters = new
            {
                p_UserId = userId,
                p_Code = code,
                p_VerificationType = verificationType.ToString()
            };

            var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "auth.sp_VerifyCode",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            if (result == null)
            {
                return Result<bool>.Failure("Verification failed");
            }

            bool isValid = result.is_valid;
            string message = result.message;

            if (!isValid)
            {
                return Result<bool>.Failure(message);
            }

            return Result<bool>.Success(true, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying code for user {UserId}", userId);
            return Result<bool>.Failure("An error occurred while verifying code");
        }
    }

    public async Task<Result<bool>> CanRequestVerificationCodeAsync(
        Guid userId,
        VerificationType verificationType)
    {
        try
        {
            using var connection = CreateConnection();
            var parameters = new
            {
                p_UserId = userId,
                p_VerificationType = verificationType.ToString(),
                p_MinutesBetweenRequests = 1
            };

            var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "auth.sp_CanRequestVerificationCode",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            if (result == null)
            {
                return Result<bool>.Failure("Failed to check rate limiting");
            }

            bool canRequest = result.can_request;
            string message = result.message;

            if (!canRequest)
            {
                return Result<bool>.Failure(message);
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking rate limit for user {UserId}", userId);
            return Result<bool>.Failure("An error occurred while checking rate limit");
        }
    }

    public async Task<Result<VerificationCode>> ResendVerificationCodeAsync(
        Guid userId,
        VerificationType verificationType,
        string target,
        string? firstName = null)
    {
        try
        {
            // Generate new code
            var code = GenerateCode();

            // Resend using stored procedure (handles rate limiting and invalidating old codes)
            using var connection = CreateConnection();
            var parameters = new
            {
                p_UserId = userId,
                p_VerificationType = verificationType.ToString(),
                p_Target = target,
                p_NewCode = code,
                p_ExpiryMinutes = 10
            };

            var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "auth.sp_ResendVerificationCode",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            if (result == null)
            {
                return Result<VerificationCode>.Failure("Failed to resend verification code");
            }

            bool canResend = result.can_resend;
            string message = result.message;

            if (!canResend)
            {
                return Result<VerificationCode>.Failure(message);
            }

            var verificationCode = new VerificationCode
            {
                VerificationCodeId = result.verification_code_id,
                UserId = userId,
                Code = result.code,
                VerificationType = verificationType,
                Target = target,
                ExpiresAt = result.expires_at,
                CreatedAt = DateTime.UtcNow
            };

            // Send via appropriate channel
            bool sent = false;
            if (verificationType == VerificationType.Email && !string.IsNullOrEmpty(firstName))
            {
                sent = await _emailService.SendVerificationCodeAsync(target, code, firstName);
            }
            else if (verificationType == VerificationType.Phone)
            {
                sent = await _smsService.SendVerificationCodeAsync(target, code);
            }

            if (!sent)
            {
                _logger.LogWarning("Failed to send verification code to {Target}, but code was created", target);
            }

            return Result<VerificationCode>.Success(verificationCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending verification code for user {UserId}", userId);
            return Result<VerificationCode>.Failure("An error occurred while resending verification code");
        }
    }
}
