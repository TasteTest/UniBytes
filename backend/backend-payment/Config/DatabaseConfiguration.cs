using backend_payment.Data;
using Microsoft.EntityFrameworkCore;

namespace backend_payment.Config;

/// <summary>
/// Database configuration extension methods
/// </summary>
public static class DatabaseConfiguration
{
    public static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__PaymentConnection")
            ?? configuration.GetConnectionString("PaymentConnection")
            ?? BuildConnectionStringFromEnv();

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'PaymentConnection' not found. " +
                "Please set it in appsettings.json or use environment variable 'ConnectionStrings__PaymentConnection'. " +
                "You can also set individual env vars: POSTGRES_HOST, POSTGRES_PORT, POSTGRES_DB, POSTGRES_USER, POSTGRES_PASSWORD");
        }

        services.AddDbContext<PaymentDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
                npgsqlOptions.CommandTimeout(30);
            });

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
        var database = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "payments_db";
        var username = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "payments_user";
        var password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");

        if (string.IsNullOrEmpty(password))
        {
            return null;
        }

        return $"Host={host};Port={port};Database={database};Username={username};Password={password}";
    }
}

