using backend.Repositories;
using backend.Repositories.Interfaces;

namespace backend.Config;

/// <summary>
/// Repository configuration and registration
/// </summary>
public static class RepositoryConfiguration
{
    public static IServiceCollection AddRepositoryConfiguration(this IServiceCollection services)
    {
        // Register generic repository
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // User service repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOAuthProviderRepository, OAuthProviderRepository>();
        services.AddScoped<IUserAnalyticsRepository, UserAnalyticsRepository>();

        // Payment service repositories
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IIdempotencyKeyRepository, IdempotencyKeyRepository>();

        // Menu service repositories
        services.AddScoped<IMenuItemRepository, MenuItemRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();

        // Loyalty service repositories
        services.AddScoped<ILoyaltyAccountRepository, LoyaltyAccountRepository>();
        services.AddScoped<ILoyaltyTransactionRepository, LoyaltyTransactionRepository>();
        services.AddScoped<ILoyaltyRedemptionRepository, LoyaltyRedemptionRepository>();

        return services;
    }
}

