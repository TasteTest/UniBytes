using backend_loyalty.Repositories;
using backend_loyalty.Repositories.Interfaces;

namespace backend_loyalty.Config;

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
        services.AddScoped<ILoyaltyAccountRepository, LoyaltyAccountRepository>();
        services.AddScoped<ILoyaltyTransactionRepository, LoyaltyTransactionRepository>();
        services.AddScoped<ILoyaltyRedemptionRepository, LoyaltyRedemptionRepository>();

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
