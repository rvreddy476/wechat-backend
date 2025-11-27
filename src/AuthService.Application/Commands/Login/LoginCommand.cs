using AuthService.Application.Common;
using MediatR;

namespace AuthService.Application.Commands.Login;

public class LoginCommand : IRequest<Result<AuthResponse>>
{
    public string UsernameOrEmail { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
