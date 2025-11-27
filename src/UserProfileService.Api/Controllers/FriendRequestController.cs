using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserProfileService.Api.Models;
using UserProfileService.Api.Repositories;
using Shared.Contracts.Common;
using Shared.Contracts.UserProfile;

namespace UserProfileService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FriendRequestController : ControllerBase
{
    private readonly IUserProfileRepository _repository;
    private readonly ILogger<FriendRequestController> _logger;

    public FriendRequestController(
        IUserProfileRepository repository,
        ILogger<FriendRequestController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException());
    }

    /// <summary>
    /// Send a friend request to another user
    /// </summary>
    [HttpPost("send/{userId:guid}")]
    public async Task<ActionResult<ApiResponse<FriendRequestDto>>> SendFriendRequest(
        Guid userId,
        [FromBody] SendFriendRequestRequest? request = null)
    {
        var currentUserId = GetCurrentUserId();

        if (currentUserId == userId)
        {
            return BadRequest(ApiResponse<FriendRequestDto>.ErrorResponse("Cannot send friend request to yourself"));
        }

        var result = await _repository.SendFriendRequestAsync(currentUserId, userId, request?.Message);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<FriendRequestDto>.ErrorResponse(result.Error));
        }

        var dto = await MapToFriendRequestDto(result.Value);
        return Ok(ApiResponse<FriendRequestDto>.SuccessResponse(dto));
    }

    /// <summary>
    /// Accept a friend request
    /// </summary>
    [HttpPost("{requestId:guid}/accept")]
    public async Task<ActionResult<ApiResponse<bool>>> AcceptFriendRequest(Guid requestId)
    {
        var currentUserId = GetCurrentUserId();
        var result = await _repository.AcceptFriendRequestAsync(requestId, currentUserId);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Friend request accepted"));
    }

    /// <summary>
    /// Reject a friend request
    /// </summary>
    [HttpPost("{requestId:guid}/reject")]
    public async Task<ActionResult<ApiResponse<bool>>> RejectFriendRequest(Guid requestId)
    {
        var currentUserId = GetCurrentUserId();
        var result = await _repository.RejectFriendRequestAsync(requestId, currentUserId);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Friend request rejected"));
    }

    /// <summary>
    /// Cancel a friend request that you sent
    /// </summary>
    [HttpDelete("{requestId:guid}/cancel")]
    public async Task<ActionResult<ApiResponse<bool>>> CancelFriendRequest(Guid requestId)
    {
        var currentUserId = GetCurrentUserId();
        var result = await _repository.CancelFriendRequestAsync(requestId, currentUserId);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Friend request cancelled"));
    }

    /// <summary>
    /// Get pending friend requests received (incoming requests)
    /// </summary>
    [HttpGet("received")]
    public async Task<ActionResult<ApiResponse<List<FriendRequestDto>>>> GetReceivedFriendRequests(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetCurrentUserId();
        var result = await _repository.GetPendingFriendRequestsReceivedAsync(userId, page, pageSize);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<List<FriendRequestDto>>.ErrorResponse(result.Error));
        }

        var dtos = new List<FriendRequestDto>();
        foreach (var request in result.Value)
        {
            dtos.Add(await MapToFriendRequestDto(request));
        }

        return Ok(ApiResponse<List<FriendRequestDto>>.SuccessResponse(dtos));
    }

    /// <summary>
    /// Get pending friend requests sent (outgoing requests)
    /// </summary>
    [HttpGet("sent")]
    public async Task<ActionResult<ApiResponse<List<FriendRequestDto>>>> GetSentFriendRequests(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetCurrentUserId();
        var result = await _repository.GetPendingFriendRequestsSentAsync(userId, page, pageSize);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<List<FriendRequestDto>>.ErrorResponse(result.Error));
        }

        var dtos = new List<FriendRequestDto>();
        foreach (var request in result.Value)
        {
            dtos.Add(await MapToFriendRequestDto(request));
        }

        return Ok(ApiResponse<List<FriendRequestDto>>.SuccessResponse(dtos));
    }

    /// <summary>
    /// Get a specific friend request by ID
    /// </summary>
    [HttpGet("{requestId:guid}")]
    public async Task<ActionResult<ApiResponse<FriendRequestDto>>> GetFriendRequest(Guid requestId)
    {
        var result = await _repository.GetFriendRequestAsync(requestId);

        if (!result.IsSuccess)
        {
            return NotFound(ApiResponse<FriendRequestDto>.ErrorResponse(result.Error));
        }

        var dto = await MapToFriendRequestDto(result.Value);
        return Ok(ApiResponse<FriendRequestDto>.SuccessResponse(dto));
    }

    /// <summary>
    /// Check friendship status with another user
    /// </summary>
    [HttpGet("status/{userId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> GetFriendshipStatus(Guid userId)
    {
        var currentUserId = GetCurrentUserId();

        if (currentUserId == userId)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Cannot check friendship status with yourself"));
        }

        var areFriends = await _repository.AreFriendsAsync(currentUserId, userId);

        if (areFriends)
        {
            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                status = "friends",
                areFriends = true,
                hasPendingRequest = false
            }));
        }

        var hasSentRequest = await _repository.HasPendingFriendRequestAsync(currentUserId, userId);
        var hasReceivedRequest = await _repository.HasPendingFriendRequestAsync(userId, currentUserId);

        if (hasSentRequest)
        {
            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                status = "request_sent",
                areFriends = false,
                hasPendingRequest = true,
                requestDirection = "outgoing"
            }));
        }

        if (hasReceivedRequest)
        {
            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                status = "request_received",
                areFriends = false,
                hasPendingRequest = true,
                requestDirection = "incoming"
            }));
        }

        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            status = "not_friends",
            areFriends = false,
            hasPendingRequest = false
        }));
    }

    /// <summary>
    /// Get list of friends
    /// </summary>
    [HttpGet("friends")]
    public async Task<ActionResult<ApiResponse<List<FriendshipDto>>>> GetFriends(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetCurrentUserId();
        var result = await _repository.GetFriendsAsync(userId, page, pageSize);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<List<FriendshipDto>>.ErrorResponse(result.Error));
        }

        var dtos = result.Value.Select(profile => new FriendshipDto
        {
            UserId = profile.UserId,
            Username = profile.Username,
            DisplayName = profile.DisplayName,
            AvatarUrl = profile.AvatarUrl,
            IsVerified = profile.IsVerified,
            Bio = profile.Bio,
            FriendshipDate = profile.CreatedAt
        }).ToList();

        return Ok(ApiResponse<List<FriendshipDto>>.SuccessResponse(dtos));
    }

    /// <summary>
    /// Get friends of a specific user
    /// </summary>
    [HttpGet("friends/{userId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<FriendshipDto>>>> GetUserFriends(
        Guid userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _repository.GetFriendsAsync(userId, page, pageSize);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<List<FriendshipDto>>.ErrorResponse(result.Error));
        }

        var dtos = result.Value.Select(profile => new FriendshipDto
        {
            UserId = profile.UserId,
            Username = profile.Username,
            DisplayName = profile.DisplayName,
            AvatarUrl = profile.AvatarUrl,
            IsVerified = profile.IsVerified,
            Bio = profile.Bio,
            FriendshipDate = profile.CreatedAt
        }).ToList();

        return Ok(ApiResponse<List<FriendshipDto>>.SuccessResponse(dtos));
    }

    /// <summary>
    /// Remove a friend (unfriend)
    /// </summary>
    [HttpDelete("friends/{userId:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> RemoveFriend(Guid userId)
    {
        var currentUserId = GetCurrentUserId();

        if (currentUserId == userId)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse("Cannot remove yourself as a friend"));
        }

        var areFriends = await _repository.AreFriendsAsync(currentUserId, userId);
        if (!areFriends)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse("You are not friends with this user"));
        }

        var result = await _repository.RemoveFriendAsync(currentUserId, userId);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Friend removed successfully"));
    }

    /// <summary>
    /// Get friend request statistics
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<FriendRequestStatsDto>>> GetFriendRequestStats()
    {
        var userId = GetCurrentUserId();

        var sentRequests = await _repository.GetPendingFriendRequestsSentAsync(userId, 1, 1000);
        var receivedRequests = await _repository.GetPendingFriendRequestsReceivedAsync(userId, 1, 1000);
        var friendsCount = await _repository.GetFriendsCountAsync(userId);

        var stats = new FriendRequestStatsDto
        {
            PendingRequestsSent = sentRequests.IsSuccess ? sentRequests.Value.Count : 0,
            PendingRequestsReceived = receivedRequests.IsSuccess ? receivedRequests.Value.Count : 0,
            TotalFriends = friendsCount.IsSuccess ? friendsCount.Value : 0
        };

        return Ok(ApiResponse<FriendRequestStatsDto>.SuccessResponse(stats));
    }

    private async Task<FriendRequestDto> MapToFriendRequestDto(FriendRequest request)
    {
        var senderProfile = await _repository.GetProfileByUserIdAsync(request.SenderId);
        var receiverProfile = await _repository.GetProfileByUserIdAsync(request.ReceiverId);

        return new FriendRequestDto
        {
            Id = request.Id,
            SenderId = request.SenderId,
            SenderUsername = senderProfile.IsSuccess ? senderProfile.Value.Username : "Unknown",
            SenderDisplayName = senderProfile.IsSuccess ? senderProfile.Value.DisplayName : "Unknown",
            SenderAvatarUrl = senderProfile.IsSuccess ? senderProfile.Value.AvatarUrl : null,
            SenderIsVerified = senderProfile.IsSuccess && senderProfile.Value.IsVerified,
            ReceiverId = request.ReceiverId,
            ReceiverUsername = receiverProfile.IsSuccess ? receiverProfile.Value.Username : "Unknown",
            ReceiverDisplayName = receiverProfile.IsSuccess ? receiverProfile.Value.DisplayName : "Unknown",
            ReceiverAvatarUrl = receiverProfile.IsSuccess ? receiverProfile.Value.AvatarUrl : null,
            ReceiverIsVerified = receiverProfile.IsSuccess && receiverProfile.Value.IsVerified,
            Status = request.Status.ToString(),
            Message = request.Message,
            CreatedAt = request.CreatedAt,
            RespondedAt = request.RespondedAt
        };
    }
}
