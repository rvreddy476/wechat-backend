using Auth.Domain.Entities;
using Shared.Domain.Common;

namespace Auth.Domain.Repositories;

public interface IUserRepository
{
    Task<Result<User>> GetByIdAsync(Guid id);
    Task<Result<User>> GetByEmailAsync(string email);
    Task<Result<User>> GetByUsernameAsync(string username);
    Task<Result<User>> GetByRefreshTokenAsync(string refreshToken);
    Task<Result<User>> CreateAsync(User user);
    Task<Result<User>> UpdateAsync(User user);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> UsernameExistsAsync(string username);
}
