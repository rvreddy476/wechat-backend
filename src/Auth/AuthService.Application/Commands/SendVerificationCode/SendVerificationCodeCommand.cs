using AuthService.Application.Common;
using MediatR;

namespace AuthService.Application.Commands.SendVerificationCode;

public class SendVerificationCodeCommand : IRequest<Result<SendVerificationCodeResponse>>
{
    public Guid UserId { get; set; }
    public string Target { get; set; } = string.Empty; // Email or Phone
    public string VerificationType { get; set; } = string.Empty; // "Email" or "Phone"
}

public class SendVerificationCodeResponse
{
    public string Message { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty; // Masked
    public DateTime ExpiresAt { get; set; }
}
