using HelpDesk.Application.Auth;
using HelpDesk.Application.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDesk.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<TaskService>();
        services.AddScoped<AuthService>();
        return services;
    }
}
