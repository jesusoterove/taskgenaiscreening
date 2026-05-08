using Microsoft.Extensions.Options;
using Npgsql;

namespace HelpDesk.Infrastructure.Persistence;

public interface IDbConnectionFactory
{
    NpgsqlConnection Create();
}

public sealed class DbConnectionFactory : IDbConnectionFactory
{
    private readonly DbOptions _options;

    public DbConnectionFactory(IOptions<DbOptions> options)
    {
        _options = options.Value;
    }

    public NpgsqlConnection Create() => new(_options.ConnectionString);
}
