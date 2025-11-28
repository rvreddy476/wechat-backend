using Npgsql;
using System.Data;

namespace Shared.Infrastructure.PostgreSQL;

/// <summary>
/// Factory for creating PostgreSQL connections
/// </summary>
public interface IPostgreSqlConnectionFactory
{
    IDbConnection CreateConnection();
}

/// <summary>
/// PostgreSQL connection factory implementation
/// </summary>
public class PostgreSqlConnectionFactory : IPostgreSqlConnectionFactory
{
    private readonly string _connectionString;

    public PostgreSqlConnectionFactory(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public IDbConnection CreateConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }
}
