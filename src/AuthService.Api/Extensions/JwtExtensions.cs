using System.Security.Claims;
using Shared.Infrastructure.Authentication;

namespace AuthService.Api.Extensions;

public static class JwtExtensions
{
    public static string? GetUserIdFromToken(this IJwtService jwtService, ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}
