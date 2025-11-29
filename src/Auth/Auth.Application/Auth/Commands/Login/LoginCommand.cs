using MediatR;
using Shared.Contracts.Auth;
using Shared.Domain.Common;

namespace Auth.Application.Auth.Commands.Login;

public record LoginCommand(string EmailOrUsername, string Password) : IRequest<Result<AuthResponse>>;
