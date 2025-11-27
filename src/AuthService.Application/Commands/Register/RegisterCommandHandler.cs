using AuthService.Application.Common;
using AuthService.Domain.Constants;
using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using AuthService.Domain.Interfaces;
using BCrypt.Net;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<RegisterResponse>>
{
    private readonly IAuthRepository _authRepository;
    private readonly IVerificationRepository _verificationRepository;
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        IAuthRepository authRepository,
        IVerificationRepository verificationRepository,
        IEmailService emailService,
        ISmsService smsService,
        ILogger<RegisterCommandHandler> logger)
    {
        _authRepository = authRepository;
        _verificationRepository = verificationRepository;
        _emailService = emailService;
        _smsService = smsService;
        _logger = logger;
    }

    public async Task<Result<RegisterResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if email already exists
            if (await _authRepository.EmailExistsAsync(request.Email))
            {
                return Result<RegisterResponse>.Failure(ErrorMessages.EmailAlreadyExists);
            }

            // Check if phone number already exists
            if (await _authRepository.PhoneNumberExistsAsync(request.PhoneNumber))
            {
                return Result<RegisterResponse>.Failure(ErrorMessages.PhoneNumberAlreadyExists);
            }

            // Check if handler already exists (if provided)
            if (!string.IsNullOrWhiteSpace(request.Handler) &&
                await _authRepository.HandlerExistsAsync(request.Handler))
            {
                return Result<RegisterResponse>.Failure(ErrorMessages.HandlerAlreadyExists);
            }

            // Auto-generate username from email (part before @)
            var baseUsername = request.Email.Split('@')[0].ToLower();
            var username = baseUsername;
            var counter = 1;

            // Ensure username is unique by appending numbers if needed
            while (await _authRepository.UsernameExistsAsync(username))
            {
                username = $"{baseUsername}{counter}";
                counter++;
            }

            // Hash password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(
                request.Password,
                AuthConstants.PasswordHashWorkFactor);

            // Parse gender enum
            if (!Enum.TryParse<GenderType>(request.Gender, ignoreCase: true, out var genderType))
            {
                return Result<RegisterResponse>.Failure(ValidationMessages.GenderInvalid);
            }

            // Create user entity
            var user = new User
            {
                UserId = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Username = username,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                PasswordHash = passwordHash,
                Handler = request.Handler,
                Gender = genderType,
                DateOfBirth = request.DateOfBirth,
                IsEmailVerified = false,
                IsPhoneVerified = false,
                IsActive = true,
                IsDeleted = false,
                FailedLoginAttempts = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Create user in database
            var userId = await _authRepository.CreateUserAsync(user);

            if (userId == Guid.Empty)
            {
                _logger.LogError("Failed to create user account for email: {Email}", request.Email);
                return Result<RegisterResponse>.Failure(ErrorMessages.UserCreationFailed);
            }

            // Send verification codes (don't fail registration if these fail)
            await SendVerificationCodesAsync(userId, request.Email, request.PhoneNumber, request.FirstName);

            // Create response
            var response = new RegisterResponse
            {
                UserId = userId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Username = user.Username,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Handler = user.Handler,
                Message = SuccessMessages.RegistrationSuccessful
            };

            return Result<RegisterResponse>.Success(response, SuccessMessages.RegistrationSuccessful);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration for email: {Email}", request.Email);
            return Result<RegisterResponse>.Failure(ErrorMessages.RegistrationFailed);
        }
    }

    private async Task SendVerificationCodesAsync(Guid userId, string email, string phoneNumber, string firstName)
    {
        try
        {
            // Generate email verification code
            var emailCode = GenerateVerificationCode();
            var emailVerification = new VerificationCode
            {
                VerificationCodeId = Guid.NewGuid(),
                UserId = userId,
                Code = emailCode,
                VerificationType = VerificationType.Email,
                Target = email,
                IsUsed = false,
                IsExpired = false,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(EmailConstants.VerificationCodeExpiryMinutes)
            };

            await _verificationRepository.CreateVerificationCodeAsync(emailVerification);
            await _emailService.SendVerificationCodeAsync(email, emailCode, firstName);

            // Generate phone verification code
            var phoneCode = GenerateVerificationCode();
            var phoneVerification = new VerificationCode
            {
                VerificationCodeId = Guid.NewGuid(),
                UserId = userId,
                Code = phoneCode,
                VerificationType = VerificationType.Phone,
                Target = phoneNumber,
                IsUsed = false,
                IsExpired = false,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(EmailConstants.VerificationCodeExpiryMinutes)
            };

            await _verificationRepository.CreateVerificationCodeAsync(phoneVerification);
            await _smsService.SendVerificationCodeAsync(phoneNumber, phoneCode);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send verification codes for user: {UserId}", userId);
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
}
