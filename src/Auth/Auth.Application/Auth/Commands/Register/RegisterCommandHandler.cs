using Auth.Domain.Entities;
using Auth.Domain.Repositories;
using MediatR;
using Shared.Contracts.Auth;
using Shared.Domain.Common;
using Shared.Infrastructure.Authentication;

namespace Auth.Application.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<AuthResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;

    public RegisterCommandHandler(IUserRepository userRepository, IJwtService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
    }

    public async Task<Result<AuthResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (await _userRepository.EmailExistsAsync(request.Email))
            return Result.Failure<AuthResponse>("Email already registered");

        if (await _userRepository.UsernameExistsAsync(request.Username))
            return Result.Failure<AuthResponse>("Username already taken");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            PhoneNumber = request.PhoneNumber,
            Roles = new() { "User" }
        };

        var refreshToken = _jwtService.GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        var result = await _userRepository.CreateAsync(user);
        if (!result.IsSuccess)
            return Result.Failure<AuthResponse>(result.Error);

        var accessToken = _jwtService.GenerateAccessToken(user.Id, user.Username, user.Email, user.Roles);

        var response = new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60),
            User = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Roles = user.Roles
            }
        };

        return Result.Success(response);
    }
}
