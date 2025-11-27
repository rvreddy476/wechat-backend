using AuthService.Application.Common;
using MediatR;

namespace AuthService.Application.Commands.VerifyCode;

public class VerifyCodeCommand : IRequest<Result<VerifyCodeResponse>>
{
    public Guid UserId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string VerificationType { get; set; } = string.Empty; // "Email" or "Phone"
}

public class VerifyCodeResponse
{
    public bool IsValid { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool EmailVerified { get; set; }
    public bool PhoneVerified { get; set; }
}
