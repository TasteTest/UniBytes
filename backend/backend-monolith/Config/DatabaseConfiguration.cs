using backend_monolith.Data;
using Microsoft.EntityFrameworkCore;

namespace backend_monolith.Config;

/// <summary>
/// Database configuration extension methods
/// </summary>
public static class DatabaseConfiguration
{
    public static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var rawConnectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        // Replace placeholder tokens with environment variables so we can
        // drive everything from a single .env.local / container env.
        var connectionString = rawConnectionString
            .Replace("${POSTGRES_HOST}", Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost")
            .Replace("${POSTGRES_PORT}", Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5432")
            .Replace("${POSTGRES_DB}", Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "restaurant_db")
            .Replace("${POSTGRES_USER}", Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "postgres")
            .Replace("${POSTGRES_PASSWORD}", Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "postgres");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        return services;
    }
}

