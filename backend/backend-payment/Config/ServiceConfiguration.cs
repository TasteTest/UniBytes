using backend_payment.Services;
using backend_payment.Services.Interfaces;

namespace backend_payment.Config;

/// <summary>
/// Service configuration extension methods
/// </summary>
public static class ServiceConfiguration
{
    public static IServiceCollection AddServiceConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IStripeService, StripeService>();
        
        // Configure HttpClient for backend-user service communication
        var userServiceUrl = configuration["Services:UserServiceUrl"] ?? "http://localhost:5000";
        services.AddHttpClient<IUserServiceClient, UserServiceClient>(client =>
        {
            client.BaseAddress = new Uri(userServiceUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        
        return services;
    }
}

