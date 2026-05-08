using System.Text;
using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Repositories;
using HelpDesk.Infrastructure.Persistence;
using Npgsql;

namespace HelpDesk.Infrastructure.Repositories;

public sealed class TaskRepository : ITaskRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public TaskRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<TaskItem> CreateAsync(TaskItem task, CancellationToken cancellationToken)
    {
        const string sql = "insert into tasks (id, title, description, status, due_date, assigned_to, created_dt, updated_dt) values (@id, @title, @description, @status, @due_date, @assigned_to, @created_dt, @updated_dt)";
        await using var connection = _connectionFactory.Create();
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        Bind(command, task);
        await command.ExecuteNonQueryAsync(cancellationToken);
        return task;
    }

    public async Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        const string sql = "select id, title, description, status, due_date, assigned_to, created_dt, updated_dt, deleted_dt from tasks where id = @id and deleted_dt is null";
        await using var connection = _connectionFactory.Create();
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", id);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }

    public async Task<IReadOnlyList<TaskItem>> ListAsync(HelpDesk.Domain.Enums.TaskStatus? status, Guid? assignedTo, DateTimeOffset? dueBefore, DateTimeOffset? dueAfter, int page, int pageSize, CancellationToken cancellationToken)
    {
        var sql = new StringBuilder("select id, title, description, status, due_date, assigned_to, created_dt, updated_dt, deleted_dt from tasks where deleted_dt is null");
        await using var connection = _connectionFactory.Create();
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand { Connection = connection };

        if (status.HasValue)
        {
            sql.Append(" and status = @status");
            command.Parameters.AddWithValue("status", status.Value.ToString());
        }

        if (assignedTo.HasValue)
        {
            sql.Append(" and assigned_to = @assigned_to");
            command.Parameters.AddWithValue("assigned_to", assignedTo.Value);
        }

        if (dueBefore.HasValue)
        {
            sql.Append(" and due_date < @due_before");
            command.Parameters.AddWithValue("due_before", dueBefore.Value);
        }

        if (dueAfter.HasValue)
        {
            sql.Append(" and due_date > @due_after");
            command.Parameters.AddWithValue("due_after", dueAfter.Value);
        }

        sql.Append(" order by due_date asc limit @limit offset @offset");
        command.Parameters.AddWithValue("limit", pageSize);
        command.Parameters.AddWithValue("offset", (page - 1) * pageSize);
        command.CommandText = sql.ToString();

        var results = new List<TaskItem>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(Map(reader));
        }

        return results;
    }

    public async Task UpdateAsync(TaskItem task, CancellationToken cancellationToken)
    {
        const string sql = "update tasks set title = @title, description = @description, status = @status, due_date = @due_date, assigned_to = @assigned_to, updated_dt = @updated_dt where id = @id and deleted_dt is null";
        await using var connection = _connectionFactory.Create();
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        Bind(command, task);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task SoftDeleteAsync(Guid id, DateTimeOffset deletedAt, CancellationToken cancellationToken)
    {
        const string sql = "update tasks set deleted_dt = @deleted_dt, updated_dt = @updated_dt where id = @id and deleted_dt is null";
        await using var connection = _connectionFactory.Create();
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("deleted_dt", deletedAt);
        command.Parameters.AddWithValue("updated_dt", deletedAt);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static void Bind(NpgsqlCommand command, TaskItem task)
    {
        command.Parameters.AddWithValue("id", task.Id);
        command.Parameters.AddWithValue("title", task.Title);
        command.Parameters.AddWithValue("description", (object?)task.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("status", task.Status.ToString());
        command.Parameters.AddWithValue("due_date", task.DueDate);
        command.Parameters.AddWithValue("assigned_to", task.AssignedToUserId);
        command.Parameters.AddWithValue("created_dt", task.CreatedDt);
        command.Parameters.AddWithValue("updated_dt", task.UpdatedDt);
    }

    private static TaskItem Map(NpgsqlDataReader reader) => new()
    {
        Id = reader.GetGuid(0),
        Title = reader.GetString(1),
        Description = reader.IsDBNull(2) ? null : reader.GetString(2),
        Status = Enum.Parse<HelpDesk.Domain.Enums.TaskStatus>(reader.GetString(3), true),
        DueDate = reader.GetFieldValue<DateTimeOffset>(4),
        AssignedToUserId = reader.GetGuid(5),
        CreatedDt = reader.GetFieldValue<DateTimeOffset>(6),
        UpdatedDt = reader.GetFieldValue<DateTimeOffset>(7),
        DeletedDt = reader.IsDBNull(8) ? null : reader.GetFieldValue<DateTimeOffset>(8)
    };
}
