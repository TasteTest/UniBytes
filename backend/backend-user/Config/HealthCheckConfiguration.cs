using backend_user.Data;
using Microsoft.EntityFrameworkCore;

namespace backend_user.Config;

/// <summary>
/// Health check configuration and registration
/// </summary>
public static class HealthCheckConfiguration
{
    public static IServiceCollection AddHealthCheckConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddHealthChecks()
            .AddNpgSql(connectionString, name: "database");

        return services;
    }

    public static IApplicationBuilder UseHealthCheckConfiguration(this IApplicationBuilder app)
    {
        app.UseHealthChecks("/health");
        return app;
    }
}

