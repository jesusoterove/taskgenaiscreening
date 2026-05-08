using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Enums;
using HelpDesk.Domain.Repositories;
using HelpDesk.Infrastructure.Persistence;
using Npgsql;

namespace HelpDesk.Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UserRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        const string sql = "select id, email, name, password_hash, role, created_dt, updated_dt from users where id = @id";
        await using var connection = _connectionFactory.Create();
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", id);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        const string sql = "select id, email, name, password_hash, role, created_dt, updated_dt from users where email = @email";
        await using var connection = _connectionFactory.Create();
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("email", email);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }

    public async Task<User> CreateAsync(User user, CancellationToken cancellationToken)
    {
        const string sql = "insert into users (id, email, name, password_hash, role, created_dt, updated_dt) values (@id, @email, @name, @password_hash, @role, @created_dt, @updated_dt)";
        await using var connection = _connectionFactory.Create();
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", user.Id);
        command.Parameters.AddWithValue("email", user.Email);
        command.Parameters.AddWithValue("name", user.Name);
        command.Parameters.AddWithValue("password_hash", user.PasswordHash);
        command.Parameters.AddWithValue("role", user.Role.ToString());
        command.Parameters.AddWithValue("created_dt", user.CreatedDt);
        command.Parameters.AddWithValue("updated_dt", user.UpdatedDt);
        await command.ExecuteNonQueryAsync(cancellationToken);
        return user;
    }

    public async Task UpdatePasswordHashAsync(Guid userId, string passwordHash, DateTimeOffset updatedAt, CancellationToken cancellationToken)
    {
        const string sql = "update users set password_hash = @password_hash, updated_dt = @updated_dt where id = @id";
        await using var connection = _connectionFactory.Create();
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", userId);
        command.Parameters.AddWithValue("password_hash", passwordHash);
        command.Parameters.AddWithValue("updated_dt", updatedAt);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static User Map(NpgsqlDataReader reader) => new()
    {
        Id = reader.GetGuid(0),
        Email = reader.GetString(1),
        Name = reader.GetString(2),
        PasswordHash = reader.GetString(3),
        Role = Enum.Parse<UserRole>(reader.GetString(4), true),
        CreatedDt = reader.GetFieldValue<DateTimeOffset>(5),
        UpdatedDt = reader.GetFieldValue<DateTimeOffset>(6)
    };
}
