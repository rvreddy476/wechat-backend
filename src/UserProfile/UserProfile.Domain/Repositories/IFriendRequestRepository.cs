using UserProfile.Domain.Entities;
using Shared.Domain.Common;

namespace UserProfile.Domain.Repositories;

public interface IFriendRequestRepository
{
    Task<Result<FriendRequest>> GetByIdAsync(string requestId);
    Task<Result<FriendRequest>> CreateAsync(FriendRequest friendRequest);
    Task<Result<FriendRequest>> UpdateAsync(FriendRequest friendRequest);
    Task<Result<List<FriendRequest>>> GetPendingRequestsAsync(Guid userId);
    Task<Result<List<FriendRequest>>> GetSentRequestsAsync(Guid userId);
    Task<Result<FriendRequest>> GetExistingRequestAsync(Guid senderId, Guid receiverId);
}
