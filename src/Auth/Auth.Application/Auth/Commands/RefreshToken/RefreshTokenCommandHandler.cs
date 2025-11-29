using Auth.Domain.Repositories;
using MediatR;
using Shared.Contracts.Auth;
using Shared.Domain.Common;
using Shared.Infrastructure.Authentication;

namespace Auth.Application.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;

    public RefreshTokenCommandHandler(IUserRepository userRepository, IJwtService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
    }

    public async Task<Result<AuthResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetByRefreshTokenAsync(request.RefreshToken);
        
        if (!users.IsSuccess)
            return Result.Failure<AuthResponse>("Invalid refresh token");

        var user = users.Value;

        if (user.RefreshTokenExpiryTime < DateTime.UtcNow)
            return Result.Failure<AuthResponse>("Refresh token expired");

        var accessToken = _jwtService.GenerateAccessToken(user.Id, user.Username, user.Email, user.Roles);
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userRepository.UpdateAsync(user);

        var response = new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
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
