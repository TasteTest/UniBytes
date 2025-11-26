using backend_loyalty.Services;
using backend_loyalty.Services.Interfaces;

namespace backend_loyalty.Config;

/// <summary>
/// Service layer configuration and registration
/// </summary>
public static class ServiceConfiguration
{
    public static IServiceCollection AddServiceConfiguration(this IServiceCollection services)
    {
        // Register application services
        services.AddScoped<ILoyaltyAccountService, LoyaltyAccountService>();

        return services;
    }
}
