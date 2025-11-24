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
public class UserProfileController : ControllerBase
{
    private readonly IUserProfileRepository _repository;
    private readonly ILogger<UserProfileController> _logger;

    public UserProfileController(
        IUserProfileRepository repository,
        ILogger<UserProfileController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException());
    }

    [HttpGet("{userId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<ProfileDto>>> GetProfile(Guid userId)
    {
        var result = await _repository.GetProfileByUserIdAsync(userId);

        if (!result.IsSuccess)
        {
            return NotFound(ApiResponse<ProfileDto>.ErrorResponse(result.Error));
        }

        var profile = result.Value;
        var dto = MapToProfileDto(profile);

        return Ok(ApiResponse<ProfileDto>.SuccessResponse(dto));
    }

    [HttpGet("username/{username}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<ProfileDto>>> GetProfileByUsername(string username)
    {
        var result = await _repository.GetProfileByUsernameAsync(username);

        if (!result.IsSuccess)
        {
            return NotFound(ApiResponse<ProfileDto>.ErrorResponse(result.Error));
        }

        var profile = result.Value;
        var dto = MapToProfileDto(profile);

        return Ok(ApiResponse<ProfileDto>.SuccessResponse(dto));
    }

    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<ProfileDto>>> GetMyProfile()
    {
        var userId = GetCurrentUserId();
        var result = await _repository.GetProfileByUserIdAsync(userId);

        if (!result.IsSuccess)
        {
            return NotFound(ApiResponse<ProfileDto>.ErrorResponse(result.Error));
        }

        var profile = result.Value;
        var dto = MapToProfileDto(profile);

        return Ok(ApiResponse<ProfileDto>.SuccessResponse(dto));
    }

    [HttpPut("me")]
    public async Task<ActionResult<ApiResponse<ProfileDto>>> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = GetCurrentUserId();

        // Get existing profile
        var profileResult = await _repository.GetProfileByUserIdAsync(userId);
        if (!profileResult.IsSuccess)
        {
            return NotFound(ApiResponse<ProfileDto>.ErrorResponse(profileResult.Error));
        }

        var profile = profileResult.Value;

        // Update fields
        if (!string.IsNullOrWhiteSpace(request.DisplayName))
            profile.DisplayName = request.DisplayName;

        if (request.Bio != null)
            profile.Bio = request.Bio;

        if (request.AvatarUrl != null)
            profile.AvatarUrl = request.AvatarUrl;

        if (request.CoverImageUrl != null)
            profile.CoverImageUrl = request.CoverImageUrl;

        if (request.Location != null)
            profile.Location = request.Location;

        if (request.Website != null)
            profile.Website = request.Website;

        if (request.DateOfBirth.HasValue)
            profile.DateOfBirth = request.DateOfBirth;

        if (request.IsPrivate.HasValue)
            profile.IsPrivate = request.IsPrivate.Value;

        // Update social links
        if (request.SocialLinks != null)
        {
            if (request.SocialLinks.Twitter != null)
                profile.SocialLinks.Twitter = request.SocialLinks.Twitter;
            if (request.SocialLinks.Instagram != null)
                profile.SocialLinks.Instagram = request.SocialLinks.Instagram;
            if (request.SocialLinks.Facebook != null)
                profile.SocialLinks.Facebook = request.SocialLinks.Facebook;
            if (request.SocialLinks.YouTube != null)
                profile.SocialLinks.YouTube = request.SocialLinks.YouTube;
            if (request.SocialLinks.TikTok != null)
                profile.SocialLinks.TikTok = request.SocialLinks.TikTok;
        }

        // Update preferences
        if (request.Preferences != null)
        {
            if (request.Preferences.ShowEmail.HasValue)
                profile.Preferences.ShowEmail = request.Preferences.ShowEmail.Value;
            if (request.Preferences.ShowDateOfBirth.HasValue)
                profile.Preferences.ShowDateOfBirth = request.Preferences.ShowDateOfBirth.Value;
            if (request.Preferences.ShowLocation.HasValue)
                profile.Preferences.ShowLocation = request.Preferences.ShowLocation.Value;
            if (request.Preferences.AllowMessagesFromNonFollowers.HasValue)
                profile.Preferences.AllowMessagesFromNonFollowers = request.Preferences.AllowMessagesFromNonFollowers.Value;
            if (request.Preferences.EmailNotifications.HasValue)
                profile.Preferences.EmailNotifications = request.Preferences.EmailNotifications.Value;
            if (request.Preferences.PushNotifications.HasValue)
                profile.Preferences.PushNotifications = request.Preferences.PushNotifications.Value;
        }

        var result = await _repository.UpdateProfileAsync(userId, profile);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<ProfileDto>.ErrorResponse(result.Error));
        }

        var dto = MapToProfileDto(profile);
        return Ok(ApiResponse<ProfileDto>.SuccessResponse(dto));
    }

    [HttpDelete("me")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteProfile()
    {
        var userId = GetCurrentUserId();
        var result = await _repository.DeleteProfileAsync(userId);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<bool>.SuccessResponse(true));
    }

    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<ProfileDto>>>> SearchProfiles(
        [FromQuery] string query,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest(ApiResponse<List<ProfileDto>>.ErrorResponse("Search query is required"));
        }

        var result = await _repository.SearchProfilesAsync(query, page, pageSize);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<List<ProfileDto>>.ErrorResponse(result.Error));
        }

        var dtos = result.Value.Select(MapToProfileDto).ToList();
        return Ok(ApiResponse<List<ProfileDto>>.SuccessResponse(dtos));
    }

    [HttpGet("suggested")]
    public async Task<ActionResult<ApiResponse<List<ProfileDto>>>> GetSuggestedProfiles([FromQuery] int limit = 10)
    {
        var userId = GetCurrentUserId();
        var result = await _repository.GetSuggestedProfilesAsync(userId, limit);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<List<ProfileDto>>.ErrorResponse(result.Error));
        }

        var dtos = result.Value.Select(MapToProfileDto).ToList();
        return Ok(ApiResponse<List<ProfileDto>>.SuccessResponse(dtos));
    }

    [HttpGet("trending")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<ProfileDto>>>> GetTrendingProfiles([FromQuery] int limit = 10)
    {
        var result = await _repository.GetTrendingProfilesAsync(limit);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<List<ProfileDto>>.ErrorResponse(result.Error));
        }

        var dtos = result.Value.Select(MapToProfileDto).ToList();
        return Ok(ApiResponse<List<ProfileDto>>.SuccessResponse(dtos));
    }

    [HttpPost("follow/{userId:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> FollowUser(Guid userId)
    {
        var currentUserId = GetCurrentUserId();

        if (currentUserId == userId)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse("Cannot follow yourself"));
        }

        // Check if blocked
        if (await _repository.IsBlockedAsync(currentUserId, userId))
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse("Cannot follow this user"));
        }

        var result = await _repository.CreateFollowAsync(currentUserId, userId);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<bool>.SuccessResponse(true));
    }

    [HttpDelete("follow/{userId:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> UnfollowUser(Guid userId)
    {
        var currentUserId = GetCurrentUserId();
        var result = await _repository.UnfollowAsync(currentUserId, userId);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<bool>.SuccessResponse(true));
    }

    [HttpPost("follow-requests/{userId:guid}/accept")]
    public async Task<ActionResult<ApiResponse<bool>>> AcceptFollowRequest(Guid userId)
    {
        var currentUserId = GetCurrentUserId();
        var result = await _repository.AcceptFollowRequestAsync(userId, currentUserId);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<bool>.SuccessResponse(true));
    }

    [HttpPost("follow-requests/{userId:guid}/reject")]
    public async Task<ActionResult<ApiResponse<bool>>> RejectFollowRequest(Guid userId)
    {
        var currentUserId = GetCurrentUserId();
        var result = await _repository.RejectFollowRequestAsync(userId, currentUserId);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<bool>.SuccessResponse(true));
    }

    [HttpGet("{userId:guid}/followers")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<ProfileDto>>>> GetFollowers(
        Guid userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _repository.GetFollowersAsync(userId, page, pageSize);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<List<ProfileDto>>.ErrorResponse(result.Error));
        }

        var dtos = result.Value.Select(MapToProfileDto).ToList();
        return Ok(ApiResponse<List<ProfileDto>>.SuccessResponse(dtos));
    }

    [HttpGet("{userId:guid}/following")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<ProfileDto>>>> GetFollowing(
        Guid userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _repository.GetFollowingAsync(userId, page, pageSize);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<List<ProfileDto>>.ErrorResponse(result.Error));
        }

        var dtos = result.Value.Select(MapToProfileDto).ToList();
        return Ok(ApiResponse<List<ProfileDto>>.SuccessResponse(dtos));
    }

    [HttpGet("follow-requests/pending")]
    public async Task<ActionResult<ApiResponse<List<ProfileDto>>>> GetPendingFollowRequests(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetCurrentUserId();
        var result = await _repository.GetPendingFollowRequestsAsync(userId, page, pageSize);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<List<ProfileDto>>.ErrorResponse(result.Error));
        }

        var dtos = result.Value.Select(MapToProfileDto).ToList();
        return Ok(ApiResponse<List<ProfileDto>>.SuccessResponse(dtos));
    }

    [HttpPost("block/{userId:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> BlockUser(Guid userId, [FromBody] BlockUserRequest? request = null)
    {
        var currentUserId = GetCurrentUserId();

        if (currentUserId == userId)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse("Cannot block yourself"));
        }

        var result = await _repository.BlockUserAsync(currentUserId, userId, request?.Reason);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<bool>.SuccessResponse(true));
    }

    [HttpDelete("block/{userId:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> UnblockUser(Guid userId)
    {
        var currentUserId = GetCurrentUserId();
        var result = await _repository.UnblockUserAsync(currentUserId, userId);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<bool>.SuccessResponse(true));
    }

    [HttpGet("blocked")]
    public async Task<ActionResult<ApiResponse<List<ProfileDto>>>> GetBlockedUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetCurrentUserId();
        var result = await _repository.GetBlockedUsersAsync(userId, page, pageSize);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<List<ProfileDto>>.ErrorResponse(result.Error));
        }

        var dtos = result.Value.Select(MapToProfileDto).ToList();
        return Ok(ApiResponse<List<ProfileDto>>.SuccessResponse(dtos));
    }

    [HttpGet("{userId:guid}/is-following")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<bool>>> IsFollowing(Guid userId)
    {
        var currentUserId = GetCurrentUserId();
        var isFollowing = await _repository.IsFollowingAsync(currentUserId, userId);

        return Ok(ApiResponse<bool>.SuccessResponse(isFollowing));
    }

    private static ProfileDto MapToProfileDto(UserProfile profile)
    {
        return new ProfileDto
        {
            UserId = profile.UserId,
            Username = profile.Username,
            Email = profile.Email,
            DisplayName = profile.DisplayName,
            Bio = profile.Bio,
            AvatarUrl = profile.AvatarUrl,
            CoverImageUrl = profile.CoverImageUrl,
            Location = profile.Location,
            Website = profile.Website,
            DateOfBirth = profile.DateOfBirth,
            IsPrivate = profile.IsPrivate,
            IsVerified = profile.IsVerified,
            Stats = new ProfileStatsDto
            {
                FollowersCount = profile.Stats.FollowersCount,
                FollowingCount = profile.Stats.FollowingCount,
                PostsCount = profile.Stats.PostsCount,
                VideosCount = profile.Stats.VideosCount,
                TotalViews = profile.Stats.TotalViews
            },
            SocialLinks = new SocialLinksDto
            {
                Twitter = profile.SocialLinks.Twitter,
                Instagram = profile.SocialLinks.Instagram,
                Facebook = profile.SocialLinks.Facebook,
                YouTube = profile.SocialLinks.YouTube,
                TikTok = profile.SocialLinks.TikTok
            },
            CreatedAt = profile.CreatedAt
        };
    }
}
