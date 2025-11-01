using backend_user.Services;
using backend_user.Services.Interfaces;

namespace backend_user.Config;

/// <summary>
/// Service layer configuration and registration
/// </summary>
public static class ServiceConfiguration
{
    public static IServiceCollection AddServiceConfiguration(this IServiceCollection services)
    {
        // Register application services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IOAuthProviderService, OAuthProviderService>();
        services.AddScoped<IUserAnalyticsService, UserAnalyticsService>();

        return services;
    }
}

