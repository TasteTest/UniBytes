using backend_user.Data;
using Microsoft.EntityFrameworkCore;

namespace backend_user.Config;

/// <summary>
/// Database configuration and registration
/// </summary>
public static class DatabaseConfiguration
{
    public static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        // Try to get connection string from environment variable first, then from configuration
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? BuildConnectionStringFromEnv();

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'DefaultConnection' not found. " +
                "Please set it in appsettings.json or use environment variable 'ConnectionStrings__DefaultConnection'. " +
                "You can also set individual env vars: POSTGRES_HOST, POSTGRES_PORT, POSTGRES_DB, POSTGRES_USER, POSTGRES_PASSWORD");
        }

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
                npgsqlOptions.CommandTimeout(30);
            });

            // Enable sensitive data logging in development
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        return services;
    }

    private static string? BuildConnectionStringFromEnv()
    {
        var host = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost";
        var port = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5432";
        var database = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "users_db";
        var username = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "users_user";
        var password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "users_pass";

        if (string.IsNullOrEmpty(password))
        {
            return null;
        }

        return $"Host={host};Port={port};Database={database};Username={username};Password={password}";
    }
}

