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

    // Friend Request Management
    Task<Result<FriendRequest>> SendFriendRequestAsync(Guid senderId, Guid receiverId, string? message = null);
    Task<Result<bool>> AcceptFriendRequestAsync(Guid requestId, Guid receiverId);
    Task<Result<bool>> RejectFriendRequestAsync(Guid requestId, Guid receiverId);
    Task<Result<bool>> CancelFriendRequestAsync(Guid requestId, Guid senderId);
    Task<Result<FriendRequest>> GetFriendRequestAsync(Guid requestId);
    Task<Result<FriendRequest>> GetFriendRequestBetweenUsersAsync(Guid senderId, Guid receiverId);
    Task<Result<List<FriendRequest>>> GetPendingFriendRequestsSentAsync(Guid userId, int page = 1, int pageSize = 20);
    Task<Result<List<FriendRequest>>> GetPendingFriendRequestsReceivedAsync(Guid userId, int page = 1, int pageSize = 20);
    Task<bool> AreFriendsAsync(Guid userId, Guid friendId);
    Task<bool> HasPendingFriendRequestAsync(Guid senderId, Guid receiverId);
    Task<Result<bool>> RemoveFriendAsync(Guid userId, Guid friendId);
    Task<Result<List<UserProfile>>> GetFriendsAsync(Guid userId, int page = 1, int pageSize = 20);
    Task<Result<int>> GetFriendsCountAsync(Guid userId);
}
