using backend_user.Repositories;
using backend_user.Repositories.Interfaces;

namespace backend_user.Config;

/// <summary>
/// Repository configuration and registration
/// </summary>
public static class RepositoryConfiguration
{
    public static IServiceCollection AddRepositoryConfiguration(this IServiceCollection services)
    {
        // Register generic repository
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // Register specific repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOAuthProviderRepository, OAuthProviderRepository>();
        services.AddScoped<IUserAnalyticsRepository, UserAnalyticsRepository>();

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}

