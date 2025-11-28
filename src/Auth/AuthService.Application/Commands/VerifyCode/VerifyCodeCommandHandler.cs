using AuthService.Application.Common;
using AuthService.Domain.Constants;
using AuthService.Domain.Enums;
using AuthService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Commands.VerifyCode;

public class VerifyCodeCommandHandler : IRequestHandler<VerifyCodeCommand, Result<VerifyCodeResponse>>
{
    private readonly IAuthRepository _authRepository;
    private readonly IVerificationRepository _verificationRepository;
    private readonly ILogger<VerifyCodeCommandHandler> _logger;

    public VerifyCodeCommandHandler(
        IAuthRepository authRepository,
        IVerificationRepository verificationRepository,
        ILogger<VerifyCodeCommandHandler> logger)
    {
        _authRepository = authRepository;
        _verificationRepository = verificationRepository;
        _logger = logger;
    }

    public async Task<Result<VerifyCodeResponse>> Handle(VerifyCodeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Parse verification type
            if (!Enum.TryParse<VerificationType>(request.VerificationType, ignoreCase: true, out var verificationType))
            {
                return Result<VerifyCodeResponse>.Failure(ValidationMessages.VerificationTypeInvalid);
            }

            // Get verification code from database
            var verificationCode = await _verificationRepository.GetVerificationCodeAsync(
                request.UserId,
                request.Code,
                verificationType);

            if (verificationCode == null)
            {
                return Result<VerifyCodeResponse>.Failure(ErrorMessages.VerificationCodeInvalid);
            }

            // Check if code is valid
            if (!verificationCode.IsValid())
            {
                if (verificationCode.IsUsed)
                {
                    return Result<VerifyCodeResponse>.Failure(ErrorMessages.VerificationCodeAlreadyUsed);
                }

                if (verificationCode.ExpiresAt < DateTime.UtcNow)
                {
                    await _verificationRepository.MarkVerificationCodeAsExpiredAsync(verificationCode.VerificationCodeId);
                    return Result<VerifyCodeResponse>.Failure(ErrorMessages.VerificationCodeExpired);
                }

                return Result<VerifyCodeResponse>.Failure(ErrorMessages.VerificationCodeInvalid);
            }

            // Mark code as used
            await _verificationRepository.MarkVerificationCodeAsUsedAsync(verificationCode.VerificationCodeId);

            // Update user verification status
            if (verificationType == VerificationType.Email)
            {
                await _authRepository.UpdateEmailVerificationStatusAsync(request.UserId, true);
            }
            else
            {
                await _authRepository.UpdatePhoneVerificationStatusAsync(request.UserId, true);
            }

            // Get updated user status
            var user = await _authRepository.GetUserByIdAsync(request.UserId);

            var response = new VerifyCodeResponse
            {
                IsValid = true,
                Message = verificationType == VerificationType.Email
                    ? SuccessMessages.EmailVerified
                    : SuccessMessages.PhoneVerified,
                EmailVerified = user?.IsEmailVerified ?? false,
                PhoneVerified = user?.IsPhoneVerified ?? false
            };

            return Result<VerifyCodeResponse>.Success(response, SuccessMessages.VerificationSuccessful);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying code for user: {UserId}", request.UserId);
            return Result<VerifyCodeResponse>.Failure(ErrorMessages.UnexpectedError);
        }
    }
}
