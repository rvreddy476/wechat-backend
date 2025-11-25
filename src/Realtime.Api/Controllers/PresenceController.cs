using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Realtime.Api.Services;
using Shared.Contracts.Common;

namespace Realtime.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PresenceController : ControllerBase
{
    private readonly IPresenceService _presenceService;
    private readonly ILogger<PresenceController> _logger;

    public PresenceController(IPresenceService presenceService, ILogger<PresenceController> logger)
    {
        _presenceService = presenceService;
        _logger = logger;
    }

    [HttpPost("check")]
    public async Task<ActionResult<ApiResponse<List<UserPresenceResponse>>>> CheckUsersPresence(
        [FromBody] CheckPresenceRequest request)
    {
        var presenceList = new List<UserPresenceResponse>();

        foreach (var userId in request.UserIds)
        {
            var isOnline = await _presenceService.IsUserOnlineAsync(userId);
            var status = await _presenceService.GetUserStatusAsync(userId);
            var lastSeen = await _presenceService.GetLastSeenAsync(userId);

            presenceList.Add(new UserPresenceResponse
            {
                UserId = userId,
                IsOnline = isOnline,
                Status = status ?? "offline",
                LastSeen = lastSeen
            });
        }

        return Ok(ApiResponse<List<UserPresenceResponse>>.SuccessResponse(presenceList));
    }

    [HttpGet("online")]
    public async Task<ActionResult<ApiResponse<List<Guid>>>> GetOnlineUsers()
    {
        var onlineUsers = await _presenceService.GetOnlineUsersAsync();
        return Ok(ApiResponse<List<Guid>>.SuccessResponse(onlineUsers));
    }

    [HttpGet("online/count")]
    public async Task<ActionResult<ApiResponse<int>>> GetOnlineCount()
    {
        var count = await _presenceService.GetOnlineCountAsync();
        return Ok(ApiResponse<int>.SuccessResponse(count));
    }

    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult<ApiResponse<UserPresenceResponse>>> GetUserPresence(Guid userId)
    {
        var isOnline = await _presenceService.IsUserOnlineAsync(userId);
        var status = await _presenceService.GetUserStatusAsync(userId);
        var lastSeen = await _presenceService.GetLastSeenAsync(userId);

        var presence = new UserPresenceResponse
        {
            UserId = userId,
            IsOnline = isOnline,
            Status = status ?? "offline",
            LastSeen = lastSeen
        };

        return Ok(ApiResponse<UserPresenceResponse>.SuccessResponse(presence));
    }
}

public record CheckPresenceRequest
{
    public required List<Guid> UserIds { get; init; }
}

public record UserPresenceResponse
{
    public required Guid UserId { get; init; }
    public required bool IsOnline { get; init; }
    public required string Status { get; init; }
    public DateTime? LastSeen { get; init; }
}
