using Auth.Application.Auth.Commands.Login;
using Auth.Application.Auth.Commands.Register;
using Auth.Application.Auth.Commands.RefreshToken;
using Auth.Application.Auth.Queries.GetCurrentUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.Auth;
using Shared.Contracts.Common;
using System.Security.Claims;

namespace Auth.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var command = new RegisterCommand(request.Username, request.Email, request.Password, request.PhoneNumber);
        var result = await _mediator.Send(command);

        return result.IsSuccess
            ? Ok(ApiResponse<AuthResponse>.SuccessResponse(result.Value))
            : BadRequest(ApiResponse<AuthResponse>.ErrorResponse(result.Error));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var command = new LoginCommand(request.EmailOrUsername, request.Password);
        var result = await _mediator.Send(command);

        return result.IsSuccess
            ? Ok(ApiResponse<AuthResponse>.SuccessResponse(result.Value))
            : BadRequest(ApiResponse<AuthResponse>.ErrorResponse(result.Error));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var command = new RefreshTokenCommand(request.RefreshToken);
        var result = await _mediator.Send(command);

        return result.IsSuccess
            ? Ok(ApiResponse<AuthResponse>.SuccessResponse(result.Value))
            : BadRequest(ApiResponse<AuthResponse>.ErrorResponse(result.Error));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("userId");
        if (userIdClaim == null)
            return Unauthorized(ApiResponse<UserDto>.ErrorResponse("User ID not found in token"));

        if (!Guid.TryParse(userIdClaim.Value, out var userId))
            return Unauthorized(ApiResponse<UserDto>.ErrorResponse("Invalid user ID in token"));

        var query = new GetCurrentUserQuery(userId);
        var result = await _mediator.Send(query);

        return result.IsSuccess
            ? Ok(ApiResponse<UserDto>.SuccessResponse(result.Value))
            : NotFound(ApiResponse<UserDto>.ErrorResponse(result.Error));
    }
}

public record RefreshTokenRequest
{
    public required string RefreshToken { get; init; }
}
