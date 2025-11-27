using AuthService.Application.Common;
using AuthService.Domain.Constants;
using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using AuthService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Commands.SendVerificationCode;

public class SendVerificationCodeCommandHandler : IRequestHandler<SendVerificationCodeCommand, Result<SendVerificationCodeResponse>>
{
    private readonly IAuthRepository _authRepository;
    private readonly IVerificationRepository _verificationRepository;
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;
    private readonly ILogger<SendVerificationCodeCommandHandler> _logger;

    public SendVerificationCodeCommandHandler(
        IAuthRepository authRepository,
        IVerificationRepository verificationRepository,
        IEmailService emailService,
        ISmsService smsService,
        ILogger<SendVerificationCodeCommandHandler> logger)
    {
        _authRepository = authRepository;
        _verificationRepository = verificationRepository;
        _emailService = emailService;
        _smsService = smsService;
        _logger = logger;
    }

    public async Task<Result<SendVerificationCodeResponse>> Handle(
        SendVerificationCodeCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Parse verification type
            if (!Enum.TryParse<VerificationType>(request.VerificationType, ignoreCase: true, out var verificationType))
            {
                return Result<SendVerificationCodeResponse>.Failure(ValidationMessages.VerificationTypeInvalid);
            }

            // Check rate limiting
            if (!await _verificationRepository.CanRequestVerificationCodeAsync(request.UserId, verificationType))
            {
                var secondsRemaining = await _verificationRepository.GetSecondsUntilNextRequestAsync(
                    request.UserId,
                    verificationType);

                var errorMessage = string.Format(ErrorMessages.VerificationCodeRateLimited, secondsRemaining);
                return Result<SendVerificationCodeResponse>.Failure(errorMessage);
            }

            // Get user details
            var user = await _authRepository.GetUserByIdAsync(request.UserId);
            if (user == null)
            {
                return Result<SendVerificationCodeResponse>.Failure(ErrorMessages.UserNotFound);
            }

            // Check if already verified
            if (verificationType == VerificationType.Email && user.IsEmailVerified)
            {
                return Result<SendVerificationCodeResponse>.Failure(ErrorMessages.VerificationAlreadyCompleted);
            }

            if (verificationType == VerificationType.Phone && user.IsPhoneVerified)
            {
                return Result<SendVerificationCodeResponse>.Failure(ErrorMessages.VerificationAlreadyCompleted);
            }

            // Invalidate old verification codes
            await _verificationRepository.InvalidateOldVerificationCodesAsync(request.UserId, verificationType);

            // Generate new verification code
            var code = GenerateVerificationCode();

            // Create verification code entity
            var verificationCode = new VerificationCode
            {
                VerificationCodeId = Guid.NewGuid(),
                UserId = request.UserId,
                Code = code,
                VerificationType = verificationType,
                Target = request.Target,
                IsUsed = false,
                IsExpired = false,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(EmailConstants.VerificationCodeExpiryMinutes)
            };

            // Save to database
            await _verificationRepository.CreateVerificationCodeAsync(verificationCode);

            // Send verification code
            if (verificationType == VerificationType.Email)
            {
                await _emailService.SendVerificationCodeAsync(request.Target, code, user.FirstName);
            }
            else
            {
                await _smsService.SendVerificationCodeAsync(request.Target, code);
            }

            // Mask the target for security
            var maskedTarget = verificationType == VerificationType.Email
                ? MaskEmail(request.Target)
                : MaskPhone(request.Target);

            var response = new SendVerificationCodeResponse
            {
                Message = string.Format(SuccessMessages.VerificationCodeSent, maskedTarget),
                Target = maskedTarget,
                ExpiresAt = verificationCode.ExpiresAt
            };

            return Result<SendVerificationCodeResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending verification code to {Target} for user: {UserId}",
                request.Target, request.UserId);
            return Result<SendVerificationCodeResponse>.Failure(ErrorMessages.UnexpectedError);
        }
    }

    private static string GenerateVerificationCode()
    {
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        var bytes = new byte[4];
        rng.GetBytes(bytes);
        var randomNumber = BitConverter.ToUInt32(bytes, 0);
        return (randomNumber % 1000000).ToString("D6");
    }

    private static string MaskEmail(string email)
    {
        var parts = email.Split('@');
        if (parts.Length != 2) return email;

        var localPart = parts[0];
        var domain = parts[1];

        if (localPart.Length <= 2)
        {
            return $"{localPart[0]}***@{domain}";
        }

        return $"{localPart[0]}***{localPart[^1]}@{domain}";
    }

    private static string MaskPhone(string phoneNumber)
    {
        if (phoneNumber.Length < 4)
        {
            return "***";
        }

        return $"***{phoneNumber[^4..]}";
    }
}
