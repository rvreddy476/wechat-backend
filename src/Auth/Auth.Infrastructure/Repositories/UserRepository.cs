using Auth.Domain.Entities;
using Auth.Domain.Repositories;
using Dapper;
using Shared.Domain.Common;
using Shared.Infrastructure.PostgreSQL;
using System.Text.Json;

namespace Auth.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UserRepository(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    public async Task<Result<User>> GetByIdAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        var user = await connection.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM users WHERE id = @Id", new { Id = id });

        return user != null ? Result.Success(user) : Result.Failure<User>("User not found");
    }

    public async Task<Result<User>> GetByEmailAsync(string email)
    {
        using var connection = _connectionFactory.CreateConnection();
        var user = await connection.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM users WHERE email = @Email", new { Email = email.ToLower() });

        return user != null ? Result.Success(user) : Result.Failure<User>("User not found");
    }

    public async Task<Result<User>> GetByUsernameAsync(string username)
    {
        using var connection = _connectionFactory.CreateConnection();
        var user = await connection.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM users WHERE username = @Username", new { Username = username });

        return user != null ? Result.Success(user) : Result.Failure<User>("User not found");
    }

    public async Task<Result<User>> CreateAsync(User user)
    {
        using var connection = _connectionFactory.CreateConnection();
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        var sql = @"
            INSERT INTO users (id, username, email, password_hash, phone_number, is_email_verified, 
                is_phone_verified, roles, refresh_token, refresh_token_expiry_time, created_at, updated_at)
            VALUES (@Id, @Username, @Email, @PasswordHash, @PhoneNumber, @IsEmailVerified, 
                @IsPhoneVerified, @Roles::jsonb, @RefreshToken, @RefreshTokenExpiryTime, @CreatedAt, @UpdatedAt)
            RETURNING *";

        var result = await connection.QueryFirstOrDefaultAsync<User>(sql, new
        {
            user.Id,
            user.Username,
            Email = user.Email.ToLower(),
            user.PasswordHash,
            user.PhoneNumber,
            user.IsEmailVerified,
            user.IsPhoneVerified,
            Roles = JsonSerializer.Serialize(user.Roles),
            user.RefreshToken,
            user.RefreshTokenExpiryTime,
            user.CreatedAt,
            user.UpdatedAt
        });

        return result != null ? Result.Success(result) : Result.Failure<User>("Failed to create user");
    }

    public async Task<Result<User>> UpdateAsync(User user)
    {
        using var connection = _connectionFactory.CreateConnection();
        user.UpdatedAt = DateTime.UtcNow;

        var sql = @"
            UPDATE users SET 
                username = @Username, email = @Email, password_hash = @PasswordHash,
                phone_number = @PhoneNumber, is_email_verified = @IsEmailVerified,
                is_phone_verified = @IsPhoneVerified, roles = @Roles::jsonb,
                refresh_token = @RefreshToken, refresh_token_expiry_time = @RefreshTokenExpiryTime,
                updated_at = @UpdatedAt
            WHERE id = @Id
            RETURNING *";

        var result = await connection.QueryFirstOrDefaultAsync<User>(sql, new
        {
            user.Id,
            user.Username,
            Email = user.Email.ToLower(),
            user.PasswordHash,
            user.PhoneNumber,
            user.IsEmailVerified,
            user.IsPhoneVerified,
            Roles = JsonSerializer.Serialize(user.Roles),
            user.RefreshToken,
            user.RefreshTokenExpiryTime,
            user.UpdatedAt
        });

        return result != null ? Result.Success(result) : Result.Failure<User>("Failed to update user");
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        using var connection = _connectionFactory.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM users WHERE email = @Email", new { Email = email.ToLower() });
        return count > 0;
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        using var connection = _connectionFactory.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM users WHERE username = @Username", new { Username = username });
        return count > 0;
    }
}
