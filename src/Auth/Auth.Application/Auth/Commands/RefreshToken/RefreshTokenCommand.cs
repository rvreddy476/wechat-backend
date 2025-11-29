using MediatR;
using Shared.Contracts.Auth;
using Shared.Domain.Common;

namespace Auth.Application.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(string RefreshToken) : IRequest<Result<AuthResponse>>;
