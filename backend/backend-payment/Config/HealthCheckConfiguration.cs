namespace backend_payment.Config;

/// <summary>
/// Health check configuration extension methods
/// </summary>
public static class HealthCheckConfiguration
{
    public static IServiceCollection AddHealthCheckConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PaymentConnection")
            ?? throw new InvalidOperationException("Connection string 'PaymentConnection' not found.");

        services.AddHealthChecks()
            .AddNpgSql(connectionString, name: "database");

        return services;
    }
}

