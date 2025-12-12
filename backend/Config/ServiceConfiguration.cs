using backend.Services;
using backend.Services.Interfaces;
using backend.Services.Wrappers;

namespace backend.Config;

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
        services.AddSingleton<IStripeServiceWrapper, StripeServiceWrapper>();
        services.AddScoped<IStripeService, StripeService>();

        // Menu services
        services.AddScoped<IMenuService, MenuService>();
        services.AddSingleton<IBlobContainerClientWrapper, BlobContainerClientWrapper>();
        services.AddScoped<IBlobStorageService, AzureBlobStorageService>();

        // Loyalty services
        services.AddScoped<ILoyaltyAccountService, LoyaltyAccountService>();

        // Order services
        services.AddScoped<IOrderService, OrderService>();

        return services;
    }
}

