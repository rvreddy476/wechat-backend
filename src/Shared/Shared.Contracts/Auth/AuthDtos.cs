namespace Shared.Contracts.Auth;

public record RegisterRequest
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string UserName {  get; init; }
    public string? Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; } = string.Empty;
    public required string Password { get; init; }
    public required string Gender { get; init; }
    public required DateTime DateOfBirth { get; init; }
    public string? Handler { get; init; }
}

public record LoginRequest
{
    public required string EmailOrUsername { get; init; }
    public required string Password { get; init; }
}

public record RegisterResponse
{
    public required string UserId { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Username { get; init; }
    public required string Email { get; init; }
    public required string PhoneNumber { get; init; }
    public string? Handler { get; init; }
    public string Message { get; init; } = "Registration successful";
}

public record LoginResponse
{
    public required string UserId { get; init; }
    public required string Username { get; init; }
    public required string Email { get; init; }
    public bool EmailVerified { get; init; }
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public DateTime ExpiresAt { get; init; }
    public List<string> Roles { get; init; } = new();
}

public record RefreshTokenRequest
{
    public required string RefreshToken { get; init; }
}

public record RefreshTokenResponse
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public DateTime ExpiresAt { get; init; }
}

public record ChangePasswordRequest
{
    public required string OldPassword { get; init; }
    public required string NewPassword { get; init; }
}

public record ForgotPasswordRequest
{
    public required string Email { get; init; }
}

public record ResetPasswordRequest
{
    public required string Token { get; init; }
    public required string NewPassword { get; init; }
}

public record VerifyEmailRequest
{
    public required string Token { get; init; }
}

// Verification Code DTOs
public record SendVerificationCodeRequest
{
    public required string Target { get; init; } // Email or Phone number
    public required string VerificationType { get; init; } // "Email" or "Phone"
}

public record VerifyCodeRequest
{
    public required string Code { get; init; } // 6-digit code
    public required string VerificationType { get; init; } // "Email" or "Phone"
}

public record SendVerificationCodeResponse
{
    public required string Message { get; init; }
    public required string Target { get; init; } // Email or phone (partially masked)
    public required DateTime ExpiresAt { get; init; }
}

public record VerifyCodeResponse
{
    public required bool IsValid { get; init; }
    public required string Message { get; init; }
    public bool EmailVerified { get; init; }
    public bool PhoneVerified { get; init; }
}
