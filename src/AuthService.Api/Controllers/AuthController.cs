using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using AuthService.Api.Extensions;
using AuthService.Api.Models;
using AuthService.Api.Repositories;
using Shared.Contracts.Auth;
using Shared.Contracts.Common;
using Shared.Infrastructure.Authentication;
using Dapper;

namespace AuthService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthRepository _authRepository;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthRepository authRepository,
        IJwtService jwtService,
        ILogger<AuthController> logger)
    {
        _authRepository = authRepository;
        _jwtService = jwtService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<RegisterResponse>>> Register([FromBody] RegisterRequest request)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.FirstName))
        {
            return BadRequest(ApiResponse<RegisterResponse>.ErrorResponse("First name is required"));
        }

        if (string.IsNullOrWhiteSpace(request.LastName))
        {
            return BadRequest(ApiResponse<RegisterResponse>.ErrorResponse("Last name is required"));
        }

        if (string.IsNullOrWhiteSpace(request.Username) || request.Username.Length < 3)
        {
            return BadRequest(ApiResponse<RegisterResponse>.ErrorResponse("Username must be at least 3 characters"));
        }

        if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains('@'))
        {
            return BadRequest(ApiResponse<RegisterResponse>.ErrorResponse("Valid email is required"));
        }

        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            return BadRequest(ApiResponse<RegisterResponse>.ErrorResponse("Phone number is required"));
        }

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
        {
            return BadRequest(ApiResponse<RegisterResponse>.ErrorResponse("Password must be at least 8 characters"));
        }

        // Validate handler if provided (optional at registration)
        if (!string.IsNullOrWhiteSpace(request.Handler) && request.Handler.Length < 3)
        {
            return BadRequest(ApiResponse<RegisterResponse>.ErrorResponse("Handler must be at least 3 characters"));
        }

        // Validate gender if provided
        if (!string.IsNullOrWhiteSpace(request.Gender))
        {
            var validGenders = new[] { "Male", "Female", "Other", "PreferNotToSay" };
            if (!validGenders.Contains(request.Gender))
            {
                return BadRequest(ApiResponse<RegisterResponse>.ErrorResponse("Invalid gender value. Must be: Male, Female, Other, or PreferNotToSay"));
            }
        }

        // Validate date of birth if provided
        if (request.DateOfBirth.HasValue && request.DateOfBirth.Value > DateTime.UtcNow.Date)
        {
            return BadRequest(ApiResponse<RegisterResponse>.ErrorResponse("Date of birth cannot be in the future"));
        }

        // Check if email or username already exists
        if (await _authRepository.EmailExistsAsync(request.Email))
        {
            return BadRequest(ApiResponse<RegisterResponse>.ErrorResponse("Email already registered"));
        }

        if (await _authRepository.UsernameExistsAsync(request.Username))
        {
            return BadRequest(ApiResponse<RegisterResponse>.ErrorResponse("Username already taken"));
        }

        // Hash password using BCrypt
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // Register user
        var result = await _authRepository.RegisterUserAsync(
            request.FirstName,
            request.LastName,
            request.Username,
            request.Email,
            request.PhoneNumber,
            passwordHash,
            request.Handler,
            request.Gender,
            request.DateOfBirth
        );

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<RegisterResponse>.ErrorResponse(result.Error));
        }

        var user = result.Value;

        // Generate email verification token
        var verificationToken = GenerateSecureToken();
        var verificationTokenHash = HashToken(verificationToken);
        var expiresAt = DateTime.UtcNow.AddDays(1);

        await _authRepository.CreateEmailVerificationTokenAsync(user.UserId, verificationTokenHash, expiresAt);

        // TODO: Send verification email with token

        var response = new RegisterResponse
        {
            UserId = user.UserId.ToString(),
            FirstName = user.FirstName,
            LastName = user.LastName,
            Username = user.Username,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber!,
            Handler = user.Handler,
            Message = "Registration successful. Please verify your email."
        };

        return Ok(ApiResponse<RegisterResponse>.SuccessResponse(response));
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.EmailOrUsername) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(ApiResponse<LoginResponse>.ErrorResponse("Email/Username and password are required"));
        }

        // Authenticate user (stored procedure handles password verification with BCrypt)
        var result = await _authRepository.AuthenticateUserAsync(request.EmailOrUsername, request.Password);

        if (!result.IsSuccess)
        {
            return Unauthorized(ApiResponse<LoginResponse>.ErrorResponse(result.Error));
        }

        var user = result.Value;

        // Check if account is locked
        if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
        {
            return Unauthorized(ApiResponse<LoginResponse>.ErrorResponse(
                $"Account is locked until {user.LockoutEnd.Value:yyyy-MM-dd HH:mm:ss} UTC"
            ));
        }

        // Generate JWT tokens
        var accessToken = _jwtService.GenerateAccessToken(
            user.UserId.ToString(),
            user.Username,
            user.Email,
            user.Roles
        );

        var refreshToken = GenerateSecureToken();
        var refreshTokenHash = HashToken(refreshToken);
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        var userAgent = Request.Headers["User-Agent"].ToString();

        // Store refresh token
        await _authRepository.CreateRefreshTokenAsync(
            user.UserId,
            refreshTokenHash,
            DateTime.UtcNow.AddDays(7),
            ipAddress,
            userAgent
        );

        var response = new LoginResponse
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            Roles = user.Roles
        };

        _logger.LogInformation("User {UserId} logged in successfully", user.UserId);

        return Ok(ApiResponse<LoginResponse>.SuccessResponse(response));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<RefreshTokenResponse>>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return BadRequest(ApiResponse<RefreshTokenResponse>.ErrorResponse("Refresh token is required"));
        }

        var tokenHash = HashToken(request.RefreshToken);

        // Validate refresh token
        var tokenResult = await _authRepository.GetRefreshTokenAsync(tokenHash);

        if (!tokenResult.IsSuccess)
        {
            return Unauthorized(ApiResponse<RefreshTokenResponse>.ErrorResponse(tokenResult.Error));
        }

        var token = tokenResult.Value;

        if (token.IsRevoked)
        {
            return Unauthorized(ApiResponse<RefreshTokenResponse>.ErrorResponse("Token has been revoked"));
        }

        if (token.ExpiresAt < DateTime.UtcNow)
        {
            return Unauthorized(ApiResponse<RefreshTokenResponse>.ErrorResponse("Token has expired"));
        }

        // Get user details
        var userResult = await _authRepository.GetUserByIdAsync(token.UserId);

        if (!userResult.IsSuccess)
        {
            return Unauthorized(ApiResponse<RefreshTokenResponse>.ErrorResponse("User not found"));
        }

        var user = userResult.Value;

        // Revoke old token
        await _authRepository.RevokeRefreshTokenAsync(token.TokenId, "Rotated");

        // Generate new tokens
        var newAccessToken = _jwtService.GenerateAccessToken(
            user.UserId.ToString(),
            user.Username,
            user.Email,
            user.Roles
        );

        var newRefreshToken = GenerateSecureToken();
        var newRefreshTokenHash = HashToken(newRefreshToken);
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        var userAgent = Request.Headers["User-Agent"].ToString();

        await _authRepository.CreateRefreshTokenAsync(
            user.UserId,
            newRefreshTokenHash,
            DateTime.UtcNow.AddDays(7),
            ipAddress,
            userAgent
        );

        var response = new RefreshTokenResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15)
        };

        return Ok(ApiResponse<RefreshTokenResponse>.SuccessResponse(response));
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<ActionResult<ApiResponse<bool>>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = _jwtService.GetUserIdFromToken(User);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<bool>.ErrorResponse("Invalid token"));
        }

        if (string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse("Current and new passwords are required"));
        }

        if (request.NewPassword.Length < 8)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse("New password must be at least 8 characters"));
        }

        var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

        var result = await _authRepository.ChangePasswordAsync(
            Guid.Parse(userId),
            request.CurrentPassword,
            newPasswordHash
        );

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(result.Error));
        }

        // Revoke all existing tokens
        await _authRepository.RevokeAllUserTokensAsync(Guid.Parse(userId), "Password changed");

        return Ok(ApiResponse<bool>.SuccessResponse(true));
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult<ApiResponse<string>>> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest(ApiResponse<string>.ErrorResponse("Email is required"));
        }

        // Check if email exists
        if (!await _authRepository.EmailExistsAsync(request.Email))
        {
            // Don't reveal if email exists or not for security
            return Ok(ApiResponse<string>.SuccessResponse("If the email exists, a password reset link has been sent"));
        }

        // Get user by email (we need to create this helper)
        using var connection = new Npgsql.NpgsqlConnection(
            HttpContext.RequestServices.GetRequiredService<IConfiguration>().GetConnectionString("PostgreSQL")
        );

        var user = await connection.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT user_id FROM auth.users WHERE email = @Email AND NOT is_deleted",
            new { Email = request.Email }
        );

        if (user != null)
        {
            var resetToken = GenerateSecureToken();
            var resetTokenHash = HashToken(resetToken);

            await _authRepository.CreatePasswordResetTokenAsync(
                user.user_id,
                resetTokenHash,
                DateTime.UtcNow.AddHours(1)
            );

            // TODO: Send password reset email with token
            _logger.LogInformation("Password reset token generated for user {UserId}", user.user_id);
        }

        return Ok(ApiResponse<string>.SuccessResponse("If the email exists, a password reset link has been sent"));
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<ApiResponse<bool>>> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse("Token and new password are required"));
        }

        if (request.NewPassword.Length < 8)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse("Password must be at least 8 characters"));
        }

        var tokenHash = HashToken(request.Token);

        // Validate token
        var tokenResult = await _authRepository.GetPasswordResetTokenAsync(tokenHash);

        if (!tokenResult.IsSuccess)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse("Invalid or expired token"));
        }

        var token = tokenResult.Value;

        // Reset password
        var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        var result = await _authRepository.ResetPasswordWithTokenAsync(token.UserId, newPasswordHash);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(result.Error));
        }

        // Mark token as used
        using var connection = new Npgsql.NpgsqlConnection(
            HttpContext.RequestServices.GetRequiredService<IConfiguration>().GetConnectionString("PostgreSQL")
        );

        await connection.ExecuteAsync(
            "UPDATE auth.password_reset_tokens SET is_used = TRUE WHERE token_id = @TokenId",
            new { TokenId = token.TokenId }
        );

        // Revoke all tokens
        await _authRepository.RevokeAllUserTokensAsync(token.UserId, "Password reset");

        return Ok(ApiResponse<bool>.SuccessResponse(true));
    }

    [HttpPost("verify-email")]
    public async Task<ActionResult<ApiResponse<bool>>> VerifyEmail([FromQuery] string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse("Token is required"));
        }

        var tokenHash = HashToken(token);

        // Validate token
        var tokenResult = await _authRepository.GetEmailVerificationTokenAsync(tokenHash);

        if (!tokenResult.IsSuccess)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse("Invalid or expired token"));
        }

        var verificationToken = tokenResult.Value;

        // Mark email as verified
        var result = await _authRepository.MarkEmailAsVerifiedAsync(verificationToken.UserId);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(result.Error));
        }

        // Mark token as used
        using var connection = new Npgsql.NpgsqlConnection(
            HttpContext.RequestServices.GetRequiredService<IConfiguration>().GetConnectionString("PostgreSQL")
        );

        await connection.ExecuteAsync(
            "UPDATE auth.email_verification_tokens SET is_used = TRUE WHERE token_id = @TokenId",
            new { TokenId = verificationToken.TokenId }
        );

        return Ok(ApiResponse<bool>.SuccessResponse(true));
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse<bool>>> Logout()
    {
        var userId = _jwtService.GetUserIdFromToken(User);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<bool>.ErrorResponse("Invalid token"));
        }

        // Revoke all tokens for this user
        await _authRepository.RevokeAllUserTokensAsync(Guid.Parse(userId), "User logout");

        return Ok(ApiResponse<bool>.SuccessResponse(true));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetCurrentUser()
    {
        var userId = _jwtService.GetUserIdFromToken(User);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<UserDto>.ErrorResponse("Invalid token"));
        }

        var result = await _authRepository.GetUserByIdAsync(Guid.Parse(userId));

        if (!result.IsSuccess)
        {
            return NotFound(ApiResponse<UserDto>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<UserDto>.SuccessResponse(result.Value));
    }

    private static string GenerateSecureToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private static string HashToken(string token)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(hashBytes);
    }
}
