using UserProfileService.Api.Models;
using Shared.Domain.Common;

namespace UserProfileService.Api.Repositories;

public interface IUserProfileRepository
{
    // Profile Management
    Task<Result<UserProfile>> CreateProfileAsync(UserProfile profile);
    Task<Result<UserProfile>> GetProfileByUserIdAsync(Guid userId);
    Task<Result<UserProfile>> GetProfileByUsernameAsync(string username);
    Task<Result<bool>> UpdateProfileAsync(Guid userId, UserProfile profile);
    Task<Result<bool>> DeleteProfileAsync(Guid userId);
    Task<bool> ProfileExistsAsync(Guid userId);

    // Search and Discovery
    Task<Result<List<UserProfile>>> SearchProfilesAsync(string searchTerm, int page = 1, int pageSize = 20);
    Task<Result<List<UserProfile>>> GetSuggestedProfilesAsync(Guid userId, int limit = 10);
    Task<Result<List<UserProfile>>> GetTrendingProfilesAsync(int limit = 10);

    // Stats Update
    Task<Result<bool>> UpdateStatsAsync(Guid userId, string statField, int incrementBy);

    // Follow Management
    Task<Result<Follow>> CreateFollowAsync(Guid followerId, Guid followeeId);
    Task<Result<bool>> AcceptFollowRequestAsync(Guid followerId, Guid followeeId);
    Task<Result<bool>> RejectFollowRequestAsync(Guid followerId, Guid followeeId);
    Task<Result<bool>> UnfollowAsync(Guid followerId, Guid followeeId);
    Task<bool> IsFollowingAsync(Guid followerId, Guid followeeId);
    Task<Result<List<UserProfile>>> GetFollowersAsync(Guid userId, int page = 1, int pageSize = 20);
    Task<Result<List<UserProfile>>> GetFollowingAsync(Guid userId, int page = 1, int pageSize = 20);
    Task<Result<List<UserProfile>>> GetPendingFollowRequestsAsync(Guid userId, int page = 1, int pageSize = 20);

    // Block Management
    Task<Result<bool>> BlockUserAsync(Guid userId, Guid blockedUserId, string? reason = null);
    Task<Result<bool>> UnblockUserAsync(Guid userId, Guid blockedUserId);
    Task<bool> IsBlockedAsync(Guid userId, Guid targetUserId);
    Task<Result<List<UserProfile>>> GetBlockedUsersAsync(Guid userId, int page = 1, int pageSize = 20);
}
