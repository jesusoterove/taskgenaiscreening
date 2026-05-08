using HelpDesk.Application.Common;
using HelpDesk.Domain.Repositories;
using HelpDesk.Infrastructure.Auth;
using HelpDesk.Infrastructure.Common;
using HelpDesk.Infrastructure.Persistence;
using HelpDesk.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDesk.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DbOptions>(configuration.GetSection(DbOptions.SectionName));
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        return services;
    }
}
