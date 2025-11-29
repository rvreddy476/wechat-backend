using Shared.Domain.Common;

namespace UserProfile.Domain.Repositories;

public interface IUserProfileRepository
{
    Task<Result<Entities.UserProfile>> GetByIdAsync(Guid userId);
    Task<Result<Entities.UserProfile>> CreateAsync(Entities.UserProfile userProfile);
    Task<Result<Entities.UserProfile>> UpdateAsync(Entities.UserProfile userProfile);
    Task<Result<List<Entities.UserProfile>>> SearchUsersAsync(string query, int page, int pageSize);
}
