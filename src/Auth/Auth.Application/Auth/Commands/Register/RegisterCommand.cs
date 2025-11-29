using MediatR;
using Shared.Contracts.Auth;
using Shared.Domain.Common;

namespace Auth.Application.Auth.Commands.Register;

public record RegisterCommand(string Username, string Email, string Password, string? PhoneNumber) 
    : IRequest<Result<AuthResponse>>;
