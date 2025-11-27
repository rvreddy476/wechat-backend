using AuthService.Application.Common;
using AuthService.Domain.Constants;
using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using AuthService.Domain.Interfaces;
using BCrypt.Net;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Infrastructure.Authentication;

namespace AuthService.Application.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponse>>
{
    private readonly IAuthRepository _authRepository;
    private readonly IJwtService _jwtService;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IAuthRepository authRepository,
        IJwtService jwtService,
        ILogger<LoginCommandHandler> logger)
    {
        _authRepository = authRepository;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<Result<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get user by email or username
            var user = await GetUserByUsernameOrEmailAsync(request.UsernameOrEmail);

            if (user == null)
            {
                _logger.LogWarning("Login attempt failed: User not found for {UsernameOrEmail}", request.UsernameOrEmail);
                return Result<AuthResponse>.Failure(ErrorMessages.InvalidCredentials);
            }

            // Check if account is locked
            if (user.IsLockedOut())
            {
                _logger.LogWarning("Login attempt failed: Account locked for user {UserId}", user.UserId);
                return Result<AuthResponse>.Failure(ErrorMessages.AccountLocked);
            }

            // Check if account is active
            if (!user.IsActive)
            {
                _logger.LogWarning("Login attempt failed: Account inactive for user {UserId}", user.UserId);
                return Result<AuthResponse>.Failure(ErrorMessages.AccountInactive);
            }

            // Check if account is deleted
            if (user.IsDeleted)
            {
                _logger.LogWarning("Login attempt failed: Account deleted for user {UserId}", user.UserId);
                return Result<AuthResponse>.Failure(ErrorMessages.AccountDeleted);
            }

            // Verify password
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

            if (!isPasswordValid)
            {
                // Increment failed login attempts
                await _authRepository.IncrementFailedLoginAttemptsAsync(user.UserId);

                _logger.LogWarning("Login attempt failed: Invalid password for user {UserId}", user.UserId);
                return Result<AuthResponse>.Failure(ErrorMessages.InvalidCredentials);
            }

            // Reset failed login attempts
            await _authRepository.ResetFailedLoginAttemptsAsync(user.UserId);

            // Update last login timestamp
            await _authRepository.UpdateLastLoginAsync(user.UserId);

            // Generate JWT access token
            var accessToken = _jwtService.GenerateAccessToken(
                user.UserId.ToString(),
                user.Username,
                user.Email,
                user.Roles);

            // Generate refresh token
            var refreshTokenValue = _jwtService.GenerateRefreshToken();
            var refreshTokenHash = HashToken(refreshTokenValue);

            // Store refresh token in database
            var refreshToken = new RefreshToken
            {
                TokenId = Guid.NewGuid(),
                UserId = user.UserId,
                TokenHash = refreshTokenHash,
                ExpiresAt = DateTime.UtcNow.AddDays(AuthConstants.RefreshTokenExpirationDays),
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow
            };

            await _authRepository.CreateRefreshTokenAsync(refreshToken);

            // Create response
            var response = new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshTokenValue,
                ExpiresAt = DateTime.UtcNow.AddMinutes(AuthConstants.AccessTokenExpirationMinutes),
                User = new UserInfo
                {
                    UserId = user.UserId,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Username = user.Username,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Handler = user.Handler,
                    Gender = user.Gender.ToString(),
                    DateOfBirth = user.DateOfBirth,
                    IsEmailVerified = user.IsEmailVerified,
                    IsPhoneVerified = user.IsPhoneVerified,
                    AvatarUrl = user.AvatarUrl,
                    Roles = user.Roles
                }
            };

            _logger.LogInformation("User {UserId} logged in successfully", user.UserId);
            return Result<AuthResponse>.Success(response, SuccessMessages.LoginSuccessful);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for {UsernameOrEmail}", request.UsernameOrEmail);
            return Result<AuthResponse>.Failure(ErrorMessages.UnexpectedError);
        }
    }

    private async Task<User?> GetUserByUsernameOrEmailAsync(string usernameOrEmail)
    {
        // Check if input is an email
        if (usernameOrEmail.Contains('@'))
        {
            return await _authRepository.GetUserByEmailAsync(usernameOrEmail);
        }

        return await _authRepository.GetUserByUsernameAsync(usernameOrEmail);
    }

    private static string HashToken(string token)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(hashBytes);
    }
}
