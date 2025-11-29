using Auth.Application.Auth.Commands.Login;
using Auth.Application.Auth.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.Auth;
using Shared.Contracts.Common;

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
}
