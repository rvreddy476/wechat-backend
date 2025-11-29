using Auth.Domain.Repositories;
using MediatR;
using Shared.Contracts.Auth;
using Shared.Domain.Common;
using Shared.Infrastructure.Authentication;

namespace Auth.Application.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;

    public LoginCommandHandler(IUserRepository userRepository, IJwtService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
    }

    public async Task<Result<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var userResult = request.EmailOrUsername.Contains('@')
            ? await _userRepository.GetByEmailAsync(request.EmailOrUsername)
            : await _userRepository.GetByUsernameAsync(request.EmailOrUsername);

        if (!userResult.IsSuccess)
            return Result.Failure<AuthResponse>("Invalid credentials");

        var user = userResult.Value;

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Result.Failure<AuthResponse>("Invalid credentials");

        var accessToken = _jwtService.GenerateAccessToken(user.Id, user.Username, user.Email, user.Roles);
        var refreshToken = _jwtService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userRepository.UpdateAsync(user);

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
