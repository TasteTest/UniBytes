using backend_monolith.Services;
using backend_monolith.Services.Interfaces;

namespace backend_monolith.Config;

/// <summary>
/// Service layer configuration and registration
/// </summary>
public static class ServiceConfiguration
{
    public static IServiceCollection AddServiceConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        // User services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IOAuthProviderService, OAuthProviderService>();
        services.AddScoped<IUserAnalyticsService, UserAnalyticsService>();

        // Payment services
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IStripeService, StripeService>();

        // Menu services
        services.AddScoped<IMenuService, MenuService>();
        services.AddScoped<IBlobStorageService, AzureBlobStorageService>();

        // Loyalty services
        services.AddScoped<ILoyaltyAccountService, LoyaltyAccountService>();

        return services;
    }
}

