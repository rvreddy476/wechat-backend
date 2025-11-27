using MongoDB.Driver;
using UserProfileService.Api.Models;
using Shared.Domain.Common;
using Shared.Infrastructure.MongoDB;

namespace UserProfileService.Api.Repositories;

public class UserProfileRepository : IUserProfileRepository
{
    private readonly IMongoCollection<UserProfile> _profiles;
    private readonly IMongoCollection<Follow> _follows;
    private readonly IMongoCollection<BlockedUser> _blockedUsers;
    private readonly IMongoCollection<FriendRequest> _friendRequests;
    private readonly IMongoCollection<Friendship> _friendships;
    private readonly ILogger<UserProfileRepository> _logger;

    public UserProfileRepository(IMongoDatabase database, ILogger<UserProfileRepository> logger)
    {
        _profiles = database.GetCollection<UserProfile>("profiles");
        _follows = database.GetCollection<Follow>("follows");
        _blockedUsers = database.GetCollection<BlockedUser>("blockedUsers");
        _friendRequests = database.GetCollection<FriendRequest>("friendRequests");
        _friendships = database.GetCollection<Friendship>("friendships");
        _logger = logger;
    }

    public async Task<Result<UserProfile>> CreateProfileAsync(UserProfile profile)
    {
        try
        {
            await _profiles.InsertOneAsync(profile);
            return Result<UserProfile>.Success(profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating profile for user {UserId}", profile.UserId);
            return Result.Failure<UserProfile>("Failed to create profile");
        }
    }

    public async Task<Result<UserProfile>> GetProfileByUserIdAsync(Guid userId)
    {
        try
        {
            var profile = await _profiles.Find(p => p.UserId == userId && !p.IsDeleted)
                .FirstOrDefaultAsync();

            if (profile == null)
            {
                return Result.Failure<UserProfile>(Errors.NotFound.Entity("User"));
            }

            return Result<UserProfile>.Success(profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting profile for user {UserId}", userId);
            return Result.Failure<UserProfile>("Failed to get profile");
        }
    }

    public async Task<Result<UserProfile>> GetProfileByUsernameAsync(string username)
    {
        try
        {
            var profile = await _profiles.Find(p => p.Username == username && !p.IsDeleted)
                .FirstOrDefaultAsync();

            if (profile == null)
            {
                return Result.Failure<UserProfile>(Errors.NotFound.Entity("User"));
            }

            return Result<UserProfile>.Success(profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting profile by username {Username}", username);
            return Result.Failure<UserProfile>("Failed to get profile");
        }
    }

    public async Task<Result<bool>> UpdateProfileAsync(Guid userId, UserProfile profile)
    {
        try
        {
            profile.UpdatedAt = DateTime.UtcNow;

            var result = await _profiles.ReplaceOneAsync(
                p => p.UserId == userId && !p.IsDeleted,
                profile
            );

            if (result.MatchedCount == 0)
            {
                return Result.Failure<bool>(Errors.NotFound.Entity("User"));
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile for user {UserId}", userId);
            return Result.Failure<bool>("Failed to update profile");
        }
    }

    public async Task<Result<bool>> DeleteProfileAsync(Guid userId)
    {
        try
        {
            var update = Builders<UserProfile>.Update
                .Set(p => p.IsDeleted, true)
                .Set(p => p.DeletedAt, DateTime.UtcNow)
                .Set(p => p.UpdatedAt, DateTime.UtcNow);

            var result = await _profiles.UpdateOneAsync(
                p => p.UserId == userId && !p.IsDeleted,
                update
            );

            if (result.MatchedCount == 0)
            {
                return Result.Failure<bool>(Errors.NotFound.Entity("User"));
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting profile for user {UserId}", userId);
            return Result.Failure<bool>("Failed to delete profile");
        }
    }

    public async Task<bool> ProfileExistsAsync(Guid userId)
    {
        try
        {
            var count = await _profiles.CountDocumentsAsync(p => p.UserId == userId && !p.IsDeleted);
            return count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if profile exists for user {UserId}", userId);
            return false;
        }
    }

    public async Task<Result<List<UserProfile>>> SearchProfilesAsync(string searchTerm, int page = 1, int pageSize = 20)
    {
        try
        {
            var filter = Builders<UserProfile>.Filter.And(
                Builders<UserProfile>.Filter.Eq(p => p.IsDeleted, false),
                Builders<UserProfile>.Filter.Or(
                    Builders<UserProfile>.Filter.Regex(p => p.Username, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                    Builders<UserProfile>.Filter.Regex(p => p.DisplayName, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"))
                )
            );

            var profiles = await _profiles.Find(filter)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            return Result<List<UserProfile>>.Success(profiles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching profiles with term {SearchTerm}", searchTerm);
            return Result.Failure<List<UserProfile>>("Failed to search profiles");
        }
    }

    public async Task<Result<List<UserProfile>>> GetSuggestedProfilesAsync(Guid userId, int limit = 10)
    {
        try
        {
            // Get profiles that the user is not following and not blocked
            var followingIds = await _follows.Find(f => f.FollowerId == userId && f.Status == FollowStatus.Accepted)
                .Project(f => f.FolloweeId)
                .ToListAsync();

            var blockedIds = await _blockedUsers.Find(b => b.UserId == userId)
                .Project(b => b.BlockedUserId)
                .ToListAsync();

            var excludedIds = followingIds.Concat(blockedIds).Concat(new[] { userId }).ToList();

            var profiles = await _profiles.Find(p => !excludedIds.Contains(p.UserId) && !p.IsDeleted)
                .SortByDescending(p => p.Stats.FollowersCount)
                .Limit(limit)
                .ToListAsync();

            return Result<List<UserProfile>>.Success(profiles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting suggested profiles for user {UserId}", userId);
            return Result.Failure<List<UserProfile>>("Failed to get suggested profiles");
        }
    }

    public async Task<Result<List<UserProfile>>> GetTrendingProfilesAsync(int limit = 10)
    {
        try
        {
            var profiles = await _profiles.Find(p => !p.IsDeleted)
                .SortByDescending(p => p.Stats.FollowersCount)
                .ThenByDescending(p => p.Stats.TotalViews)
                .Limit(limit)
                .ToListAsync();

            return Result<List<UserProfile>>.Success(profiles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting trending profiles");
            return Result.Failure<List<UserProfile>>("Failed to get trending profiles");
        }
    }

    public async Task<Result<bool>> UpdateStatsAsync(Guid userId, string statField, int incrementBy)
    {
        try
        {
            var update = Builders<UserProfile>.Update
                .Inc($"stats.{statField}", incrementBy)
                .Set(p => p.UpdatedAt, DateTime.UtcNow);

            var result = await _profiles.UpdateOneAsync(
                p => p.UserId == userId && !p.IsDeleted,
                update
            );

            if (result.MatchedCount == 0)
            {
                return Result.Failure<bool>(Errors.NotFound.Entity("User"));
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating stats for user {UserId}", userId);
            return Result.Failure<bool>("Failed to update stats");
        }
    }

    public async Task<Result<Follow>> CreateFollowAsync(Guid followerId, Guid followeeId)
    {
        try
        {
            // Check if already following
            var existing = await _follows.Find(f =>
                f.FollowerId == followerId &&
                f.FolloweeId == followeeId &&
                !f.IsDeleted
            ).FirstOrDefaultAsync();

            if (existing != null)
            {
                return Result.Failure<Follow>("Already following this user");
            }

            // Check if followee profile is private
            var followeeProfile = await GetProfileByUserIdAsync(followeeId);
            if (!followeeProfile.IsSuccess)
            {
                return Result.Failure<Follow>("User not found");
            }

            var follow = new Follow
            {
                FollowerId = followerId,
                FolloweeId = followeeId,
                Status = followeeProfile.Value.IsPrivate ? FollowStatus.Pending : FollowStatus.Accepted
            };

            await _follows.InsertOneAsync(follow);

            // If auto-accepted, update follower counts
            if (follow.Status == FollowStatus.Accepted)
            {
                await UpdateStatsAsync(followerId, "followingCount", 1);
                await UpdateStatsAsync(followeeId, "followersCount", 1);
            }

            return Result<Follow>.Success(follow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating follow {FollowerId} -> {FolloweeId}", followerId, followeeId);
            return Result.Failure<Follow>("Failed to follow user");
        }
    }

    public async Task<Result<bool>> AcceptFollowRequestAsync(Guid followerId, Guid followeeId)
    {
        try
        {
            var update = Builders<Follow>.Update
                .Set(f => f.Status, FollowStatus.Accepted)
                .Set(f => f.UpdatedAt, DateTime.UtcNow);

            var result = await _follows.UpdateOneAsync(
                f => f.FollowerId == followerId && f.FolloweeId == followeeId && f.Status == FollowStatus.Pending,
                update
            );

            if (result.MatchedCount == 0)
            {
                return Result.Failure<bool>("Follow request not found");
            }

            // Update follower counts
            await UpdateStatsAsync(followerId, "followingCount", 1);
            await UpdateStatsAsync(followeeId, "followersCount", 1);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting follow request {FollowerId} -> {FolloweeId}", followerId, followeeId);
            return Result.Failure<bool>("Failed to accept follow request");
        }
    }

    public async Task<Result<bool>> RejectFollowRequestAsync(Guid followerId, Guid followeeId)
    {
        try
        {
            var result = await _follows.DeleteOneAsync(
                f => f.FollowerId == followerId && f.FolloweeId == followeeId && f.Status == FollowStatus.Pending
            );

            if (result.DeletedCount == 0)
            {
                return Result.Failure<bool>("Follow request not found");
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting follow request {FollowerId} -> {FolloweeId}", followerId, followeeId);
            return Result.Failure<bool>("Failed to reject follow request");
        }
    }

    public async Task<Result<bool>> UnfollowAsync(Guid followerId, Guid followeeId)
    {
        try
        {
            var follow = await _follows.Find(f =>
                f.FollowerId == followerId &&
                f.FolloweeId == followeeId &&
                !f.IsDeleted
            ).FirstOrDefaultAsync();

            if (follow == null)
            {
                return Result.Failure<bool>("Not following this user");
            }

            var update = Builders<Follow>.Update
                .Set(f => f.IsDeleted, true)
                .Set(f => f.DeletedAt, DateTime.UtcNow)
                .Set(f => f.UpdatedAt, DateTime.UtcNow);

            await _follows.UpdateOneAsync(f => f.Id == follow.Id, update);

            // Update follower counts only if the follow was accepted
            if (follow.Status == FollowStatus.Accepted)
            {
                await UpdateStatsAsync(followerId, "followingCount", -1);
                await UpdateStatsAsync(followeeId, "followersCount", -1);
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unfollowing {FollowerId} -> {FolloweeId}", followerId, followeeId);
            return Result.Failure<bool>("Failed to unfollow user");
        }
    }

    public async Task<bool> IsFollowingAsync(Guid followerId, Guid followeeId)
    {
        try
        {
            var count = await _follows.CountDocumentsAsync(f =>
                f.FollowerId == followerId &&
                f.FolloweeId == followeeId &&
                f.Status == FollowStatus.Accepted &&
                !f.IsDeleted
            );

            return count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if following {FollowerId} -> {FolloweeId}", followerId, followeeId);
            return false;
        }
    }

    public async Task<Result<List<UserProfile>>> GetFollowersAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        try
        {
            var followerIds = await _follows.Find(f =>
                    f.FolloweeId == userId &&
                    f.Status == FollowStatus.Accepted &&
                    !f.IsDeleted
                )
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .Project(f => f.FollowerId)
                .ToListAsync();

            var profiles = await _profiles.Find(p => followerIds.Contains(p.UserId) && !p.IsDeleted)
                .ToListAsync();

            return Result<List<UserProfile>>.Success(profiles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting followers for user {UserId}", userId);
            return Result.Failure<List<UserProfile>>("Failed to get followers");
        }
    }

    public async Task<Result<List<UserProfile>>> GetFollowingAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        try
        {
            var followingIds = await _follows.Find(f =>
                    f.FollowerId == userId &&
                    f.Status == FollowStatus.Accepted &&
                    !f.IsDeleted
                )
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .Project(f => f.FolloweeId)
                .ToListAsync();

            var profiles = await _profiles.Find(p => followingIds.Contains(p.UserId) && !p.IsDeleted)
                .ToListAsync();

            return Result<List<UserProfile>>.Success(profiles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting following for user {UserId}", userId);
            return Result.Failure<List<UserProfile>>("Failed to get following");
        }
    }

    public async Task<Result<List<UserProfile>>> GetPendingFollowRequestsAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        try
        {
            var requesterIds = await _follows.Find(f =>
                    f.FolloweeId == userId &&
                    f.Status == FollowStatus.Pending &&
                    !f.IsDeleted
                )
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .Project(f => f.FollowerId)
                .ToListAsync();

            var profiles = await _profiles.Find(p => requesterIds.Contains(p.UserId) && !p.IsDeleted)
                .ToListAsync();

            return Result<List<UserProfile>>.Success(profiles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending follow requests for user {UserId}", userId);
            return Result.Failure<List<UserProfile>>("Failed to get pending requests");
        }
    }

    public async Task<Result<bool>> BlockUserAsync(Guid userId, Guid blockedUserId, string? reason = null)
    {
        try
        {
            // Check if already blocked
            var existing = await _blockedUsers.Find(b =>
                b.UserId == userId &&
                b.BlockedUserId == blockedUserId
            ).FirstOrDefaultAsync();

            if (existing != null)
            {
                return Result.Failure<bool>("User is already blocked");
            }

            var blockedUser = new BlockedUser
            {
                UserId = userId,
                BlockedUserId = blockedUserId,
                Reason = reason
            };

            await _blockedUsers.InsertOneAsync(blockedUser);

            // Remove follow relationships
            await UnfollowAsync(userId, blockedUserId);
            await UnfollowAsync(blockedUserId, userId);

            // Remove friendship if exists
            await RemoveFriendAsync(userId, blockedUserId);

            // Cancel any pending friend requests
            var pendingRequestsFromBlocker = await _friendRequests.Find(fr =>
                fr.SenderId == userId &&
                fr.ReceiverId == blockedUserId &&
                fr.Status == FriendRequestStatus.Pending &&
                !fr.IsDeleted
            ).ToListAsync();

            foreach (var request in pendingRequestsFromBlocker)
            {
                request.Status = FriendRequestStatus.Cancelled;
                request.UpdatedAt = DateTime.UtcNow;
                await _friendRequests.ReplaceOneAsync(fr => fr.Id == request.Id, request);
            }

            var pendingRequestsFromBlocked = await _friendRequests.Find(fr =>
                fr.SenderId == blockedUserId &&
                fr.ReceiverId == userId &&
                fr.Status == FriendRequestStatus.Pending &&
                !fr.IsDeleted
            ).ToListAsync();

            foreach (var request in pendingRequestsFromBlocked)
            {
                request.Status = FriendRequestStatus.Rejected;
                request.RespondedAt = DateTime.UtcNow;
                request.UpdatedAt = DateTime.UtcNow;
                await _friendRequests.ReplaceOneAsync(fr => fr.Id == request.Id, request);
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error blocking user {UserId} -> {BlockedUserId}", userId, blockedUserId);
            return Result.Failure<bool>("Failed to block user");
        }
    }

    public async Task<Result<bool>> UnblockUserAsync(Guid userId, Guid blockedUserId)
    {
        try
        {
            var result = await _blockedUsers.DeleteOneAsync(b =>
                b.UserId == userId &&
                b.BlockedUserId == blockedUserId
            );

            if (result.DeletedCount == 0)
            {
                return Result.Failure<bool>("User is not blocked");
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unblocking user {UserId} -> {BlockedUserId}", userId, blockedUserId);
            return Result.Failure<bool>("Failed to unblock user");
        }
    }

    public async Task<bool> IsBlockedAsync(Guid userId, Guid targetUserId)
    {
        try
        {
            var count = await _blockedUsers.CountDocumentsAsync(b =>
                (b.UserId == userId && b.BlockedUserId == targetUserId) ||
                (b.UserId == targetUserId && b.BlockedUserId == userId)
            );

            return count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if blocked {UserId} <-> {TargetUserId}", userId, targetUserId);
            return false;
        }
    }

    public async Task<Result<List<UserProfile>>> GetBlockedUsersAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        try
        {
            var blockedIds = await _blockedUsers.Find(b => b.UserId == userId)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .Project(b => b.BlockedUserId)
                .ToListAsync();

            var profiles = await _profiles.Find(p => blockedIds.Contains(p.UserId) && !p.IsDeleted)
                .ToListAsync();

            return Result<List<UserProfile>>.Success(profiles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting blocked users for user {UserId}", userId);
            return Result.Failure<List<UserProfile>>("Failed to get blocked users");
        }
    }

    // Friend Request Management
    public async Task<Result<FriendRequest>> SendFriendRequestAsync(Guid senderId, Guid receiverId, string? message = null)
    {
        try
        {
            // Cannot send friend request to yourself
            if (senderId == receiverId)
            {
                return Result<FriendRequest>.Failure("Cannot send friend request to yourself");
            }

            // Check if blocked
            if (await IsBlockedAsync(senderId, receiverId))
            {
                return Result<FriendRequest>.Failure("Cannot send friend request to this user");
            }

            // Check if already friends
            if (await AreFriendsAsync(senderId, receiverId))
            {
                return Result<FriendRequest>.Failure("You are already friends with this user");
            }

            // Check if there's already a pending request
            var existingRequest = await _friendRequests.Find(fr =>
                ((fr.SenderId == senderId && fr.ReceiverId == receiverId) ||
                 (fr.SenderId == receiverId && fr.ReceiverId == senderId)) &&
                fr.Status == FriendRequestStatus.Pending &&
                !fr.IsDeleted
            ).FirstOrDefaultAsync();

            if (existingRequest != null)
            {
                if (existingRequest.SenderId == senderId)
                {
                    return Result<FriendRequest>.Failure("Friend request already sent");
                }
                else
                {
                    return Result<FriendRequest>.Failure("This user has already sent you a friend request");
                }
            }

            // Verify both users exist
            var senderProfile = await GetProfileByUserIdAsync(senderId);
            var receiverProfile = await GetProfileByUserIdAsync(receiverId);

            if (!senderProfile.IsSuccess || !receiverProfile.IsSuccess)
            {
                return Result<FriendRequest>.Failure("User not found");
            }

            var friendRequest = new FriendRequest
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Message = message,
                Status = FriendRequestStatus.Pending
            };

            await _friendRequests.InsertOneAsync(friendRequest);

            return Result<FriendRequest>.Success(friendRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending friend request from {SenderId} to {ReceiverId}", senderId, receiverId);
            return Result<FriendRequest>.Failure("Failed to send friend request");
        }
    }

    public async Task<Result<bool>> AcceptFriendRequestAsync(Guid requestId, Guid receiverId)
    {
        try
        {
            var requestObjectId = MongoDB.Bson.ObjectId.Parse(requestId.ToString("N").Substring(0, 24));
            var requestIdString = requestObjectId.ToString();

            var friendRequest = await _friendRequests.Find(fr =>
                fr.Id == requestIdString &&
                fr.ReceiverId == receiverId &&
                fr.Status == FriendRequestStatus.Pending &&
                !fr.IsDeleted
            ).FirstOrDefaultAsync();

            if (friendRequest == null)
            {
                return Result<bool>.Failure("Friend request not found");
            }

            // Update friend request status
            friendRequest.Status = FriendRequestStatus.Accepted;
            friendRequest.RespondedAt = DateTime.UtcNow;
            friendRequest.UpdatedAt = DateTime.UtcNow;

            await _friendRequests.ReplaceOneAsync(fr => fr.Id == requestIdString, friendRequest);

            // Create mutual friendships
            var friendship1 = new Friendship
            {
                UserId = friendRequest.SenderId,
                FriendId = friendRequest.ReceiverId,
                FriendshipDate = DateTime.UtcNow
            };

            var friendship2 = new Friendship
            {
                UserId = friendRequest.ReceiverId,
                FriendId = friendRequest.SenderId,
                FriendshipDate = DateTime.UtcNow
            };

            await _friendships.InsertOneAsync(friendship1);
            await _friendships.InsertOneAsync(friendship2);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting friend request {RequestId}", requestId);
            return Result<bool>.Failure("Failed to accept friend request");
        }
    }

    public async Task<Result<bool>> RejectFriendRequestAsync(Guid requestId, Guid receiverId)
    {
        try
        {
            var requestObjectId = MongoDB.Bson.ObjectId.Parse(requestId.ToString("N").Substring(0, 24));
            var requestIdString = requestObjectId.ToString();

            var friendRequest = await _friendRequests.Find(fr =>
                fr.Id == requestIdString &&
                fr.ReceiverId == receiverId &&
                fr.Status == FriendRequestStatus.Pending &&
                !fr.IsDeleted
            ).FirstOrDefaultAsync();

            if (friendRequest == null)
            {
                return Result<bool>.Failure("Friend request not found");
            }

            friendRequest.Status = FriendRequestStatus.Rejected;
            friendRequest.RespondedAt = DateTime.UtcNow;
            friendRequest.UpdatedAt = DateTime.UtcNow;

            await _friendRequests.ReplaceOneAsync(fr => fr.Id == requestIdString, friendRequest);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting friend request {RequestId}", requestId);
            return Result<bool>.Failure("Failed to reject friend request");
        }
    }

    public async Task<Result<bool>> CancelFriendRequestAsync(Guid requestId, Guid senderId)
    {
        try
        {
            var requestObjectId = MongoDB.Bson.ObjectId.Parse(requestId.ToString("N").Substring(0, 24));
            var requestIdString = requestObjectId.ToString();

            var friendRequest = await _friendRequests.Find(fr =>
                fr.Id == requestIdString &&
                fr.SenderId == senderId &&
                fr.Status == FriendRequestStatus.Pending &&
                !fr.IsDeleted
            ).FirstOrDefaultAsync();

            if (friendRequest == null)
            {
                return Result<bool>.Failure("Friend request not found");
            }

            friendRequest.Status = FriendRequestStatus.Cancelled;
            friendRequest.UpdatedAt = DateTime.UtcNow;

            await _friendRequests.ReplaceOneAsync(fr => fr.Id == requestIdString, friendRequest);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling friend request {RequestId}", requestId);
            return Result<bool>.Failure("Failed to cancel friend request");
        }
    }

    public async Task<Result<FriendRequest>> GetFriendRequestAsync(Guid requestId)
    {
        try
        {
            var requestObjectId = MongoDB.Bson.ObjectId.Parse(requestId.ToString("N").Substring(0, 24));
            var requestIdString = requestObjectId.ToString();

            var friendRequest = await _friendRequests.Find(fr => fr.Id == requestIdString && !fr.IsDeleted)
                .FirstOrDefaultAsync();

            if (friendRequest == null)
            {
                return Result<FriendRequest>.Failure("Friend request not found");
            }

            return Result<FriendRequest>.Success(friendRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting friend request {RequestId}", requestId);
            return Result<FriendRequest>.Failure("Failed to get friend request");
        }
    }

    public async Task<Result<FriendRequest>> GetFriendRequestBetweenUsersAsync(Guid senderId, Guid receiverId)
    {
        try
        {
            var friendRequest = await _friendRequests.Find(fr =>
                fr.SenderId == senderId &&
                fr.ReceiverId == receiverId &&
                fr.Status == FriendRequestStatus.Pending &&
                !fr.IsDeleted
            ).FirstOrDefaultAsync();

            if (friendRequest == null)
            {
                return Result<FriendRequest>.Failure("Friend request not found");
            }

            return Result<FriendRequest>.Success(friendRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting friend request between {SenderId} and {ReceiverId}", senderId, receiverId);
            return Result<FriendRequest>.Failure("Failed to get friend request");
        }
    }

    public async Task<Result<List<FriendRequest>>> GetPendingFriendRequestsSentAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        try
        {
            var requests = await _friendRequests.Find(fr =>
                    fr.SenderId == userId &&
                    fr.Status == FriendRequestStatus.Pending &&
                    !fr.IsDeleted
                )
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .SortByDescending(fr => fr.CreatedAt)
                .ToListAsync();

            return Result<List<FriendRequest>>.Success(requests);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending friend requests sent by {UserId}", userId);
            return Result<List<FriendRequest>>.Failure("Failed to get pending friend requests");
        }
    }

    public async Task<Result<List<FriendRequest>>> GetPendingFriendRequestsReceivedAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        try
        {
            var requests = await _friendRequests.Find(fr =>
                    fr.ReceiverId == userId &&
                    fr.Status == FriendRequestStatus.Pending &&
                    !fr.IsDeleted
                )
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .SortByDescending(fr => fr.CreatedAt)
                .ToListAsync();

            return Result<List<FriendRequest>>.Success(requests);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending friend requests received by {UserId}", userId);
            return Result<List<FriendRequest>>.Failure("Failed to get pending friend requests");
        }
    }

    public async Task<bool> AreFriendsAsync(Guid userId, Guid friendId)
    {
        try
        {
            var count = await _friendships.CountDocumentsAsync(f =>
                f.UserId == userId &&
                f.FriendId == friendId &&
                !f.IsDeleted
            );

            return count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if friends {UserId} <-> {FriendId}", userId, friendId);
            return false;
        }
    }

    public async Task<bool> HasPendingFriendRequestAsync(Guid senderId, Guid receiverId)
    {
        try
        {
            var count = await _friendRequests.CountDocumentsAsync(fr =>
                fr.SenderId == senderId &&
                fr.ReceiverId == receiverId &&
                fr.Status == FriendRequestStatus.Pending &&
                !fr.IsDeleted
            );

            return count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking pending friend request {SenderId} -> {ReceiverId}", senderId, receiverId);
            return false;
        }
    }

    public async Task<Result<bool>> RemoveFriendAsync(Guid userId, Guid friendId)
    {
        try
        {
            // Delete both friendship records
            await _friendships.DeleteOneAsync(f =>
                f.UserId == userId &&
                f.FriendId == friendId
            );

            await _friendships.DeleteOneAsync(f =>
                f.UserId == friendId &&
                f.FriendId == userId
            );

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing friend {UserId} <-> {FriendId}", userId, friendId);
            return Result<bool>.Failure("Failed to remove friend");
        }
    }

    public async Task<Result<List<UserProfile>>> GetFriendsAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        try
        {
            var friendIds = await _friendships.Find(f =>
                    f.UserId == userId &&
                    !f.IsDeleted
                )
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .SortByDescending(f => f.FriendshipDate)
                .Project(f => f.FriendId)
                .ToListAsync();

            var profiles = await _profiles.Find(p => friendIds.Contains(p.UserId) && !p.IsDeleted)
                .ToListAsync();

            return Result<List<UserProfile>>.Success(profiles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting friends for user {UserId}", userId);
            return Result<List<UserProfile>>.Failure("Failed to get friends");
        }
    }

    public async Task<Result<int>> GetFriendsCountAsync(Guid userId)
    {
        try
        {
            var count = await _friendships.CountDocumentsAsync(f =>
                f.UserId == userId &&
                !f.IsDeleted
            );

            return Result<int>.Success((int)count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting friends count for user {UserId}", userId);
            return Result<int>.Failure("Failed to get friends count");
        }
    }
}
