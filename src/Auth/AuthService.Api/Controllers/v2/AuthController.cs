using AuthService.Application.Commands.Login;
using AuthService.Application.Commands.Register;
using AuthService.Application.Commands.SendVerificationCode;
using AuthService.Application.Commands.VerifyCode;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.Auth;
using Shared.Contracts.Common;
using Shared.Infrastructure.Authentication;

namespace AuthService.Api.Controllers.v2;

[ApiController]
[Route("api/v2/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IMediator mediator,
        IJwtService jwtService,
        ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _jwtService = jwtService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<RegisterResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<RegisterResponse>), 400)]
    public async Task<ActionResult<ApiResponse<RegisterResponse>>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var command = new RegisterCommand
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Password = request.Password,
                Gender = request.Gender,
                DateOfBirth = request.DateOfBirth,
                Handler = request.Handler
            };

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<RegisterResponse>.ErrorResponse(result.Error!));
            }

            return Ok(ApiResponse<RegisterResponse>.SuccessResponse(result.Value!, result.Message));
        }
        catch (FluentValidation.ValidationException ex)
        {
            var errors = string.Join("; ", ex.Errors.Select(e => e.ErrorMessage));
            return BadRequest(ApiResponse<RegisterResponse>.ErrorResponse(errors));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return StatusCode(500, ApiResponse<RegisterResponse>.ErrorResponse("An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Login with username/email and password
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), 400)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var command = new LoginCommand
            {
                UsernameOrEmail = request.UsernameOrEmail,
                Password = request.Password
            };

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<AuthResponse>.ErrorResponse(result.Error!));
            }

            // Map to contracts AuthResponse
            var response = new Shared.Contracts.Auth.AuthResponse
            {
                AccessToken = result.Value!.AccessToken,
                RefreshToken = result.Value!.RefreshToken,
                ExpiresAt = result.Value!.ExpiresAt,
                User = new Shared.Contracts.Auth.UserInfo
                {
                    UserId = result.Value!.User.UserId,
                    FirstName = result.Value!.User.FirstName,
                    LastName = result.Value!.User.LastName,
                    Username = result.Value!.User.Username,
                    Email = result.Value!.User.Email,
                    PhoneNumber = result.Value!.User.PhoneNumber,
                    Handler = result.Value!.User.Handler,
                    Gender = result.Value!.User.Gender,
                    DateOfBirth = result.Value!.User.DateOfBirth,
                    IsEmailVerified = result.Value!.User.IsEmailVerified,
                    IsPhoneVerified = result.Value!.User.IsPhoneVerified,
                    AvatarUrl = result.Value!.User.AvatarUrl,
                    Roles = result.Value!.User.Roles
                }
            };

            return Ok(ApiResponse<Shared.Contracts.Auth.AuthResponse>.SuccessResponse(response, result.Message));
        }
        catch (FluentValidation.ValidationException ex)
        {
            var errors = string.Join("; ", ex.Errors.Select(e => e.ErrorMessage));
            return BadRequest(ApiResponse<Shared.Contracts.Auth.AuthResponse>.ErrorResponse(errors));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, ApiResponse<Shared.Contracts.Auth.AuthResponse>.ErrorResponse("An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Send verification code to email or phone
    /// </summary>
    [Authorize]
    [HttpPost("send-verification-code")]
    [ProducesResponseType(typeof(ApiResponse<SendVerificationCodeResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<SendVerificationCodeResponse>), 400)]
    public async Task<ActionResult<ApiResponse<SendVerificationCodeResponse>>> SendVerificationCode(
        [FromBody] SendVerificationCodeRequest request)
    {
        try
        {
            var userId = _jwtService.GetUserIdFromToken(User);

            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return Unauthorized(ApiResponse<SendVerificationCodeResponse>.ErrorResponse("Invalid or missing user token"));
            }

            var command = new SendVerificationCodeCommand
            {
                UserId = userGuid,
                Target = request.Target,
                VerificationType = request.VerificationType
            };

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<SendVerificationCodeResponse>.ErrorResponse(result.Error!));
            }

            return Ok(ApiResponse<SendVerificationCodeResponse>.SuccessResponse(result.Value!));
        }
        catch (FluentValidation.ValidationException ex)
        {
            var errors = string.Join("; ", ex.Errors.Select(e => e.ErrorMessage));
            return BadRequest(ApiResponse<SendVerificationCodeResponse>.ErrorResponse(errors));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending verification code");
            return StatusCode(500, ApiResponse<SendVerificationCodeResponse>.ErrorResponse("An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Verify email or phone with 6-digit code
    /// </summary>
    [Authorize]
    [HttpPost("verify-code")]
    [ProducesResponseType(typeof(ApiResponse<VerifyCodeResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<VerifyCodeResponse>), 400)]
    public async Task<ActionResult<ApiResponse<VerifyCodeResponse>>> VerifyCode(
        [FromBody] VerifyCodeRequest request)
    {
        try
        {
            var userId = _jwtService.GetUserIdFromToken(User);

            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return Unauthorized(ApiResponse<VerifyCodeResponse>.ErrorResponse("Invalid or missing user token"));
            }

            var command = new VerifyCodeCommand
            {
                UserId = userGuid,
                Code = request.Code,
                VerificationType = request.VerificationType
            };

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<VerifyCodeResponse>.ErrorResponse(result.Error!));
            }

            return Ok(ApiResponse<VerifyCodeResponse>.SuccessResponse(result.Value!, result.Message));
        }
        catch (FluentValidation.ValidationException ex)
        {
            var errors = string.Join("; ", ex.Errors.Select(e => e.ErrorMessage));
            return BadRequest(ApiResponse<VerifyCodeResponse>.ErrorResponse(errors));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying code");
            return StatusCode(500, ApiResponse<VerifyCodeResponse>.ErrorResponse("An unexpected error occurred"));
        }
    }
}
