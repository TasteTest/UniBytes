namespace backend_payment.Config;

/// <summary>
/// CORS configuration extension methods
/// </summary>
public static class CorsConfiguration
{
    private const string AllowFrontendPolicy = "AllowFrontend";

    public static IServiceCollection AddCorsConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var frontendUrl = configuration["Frontend:Url"] ?? "http://localhost:3000";

        services.AddCors(options =>
        {
            options.AddPolicy(AllowFrontendPolicy, builder =>
            {
                builder.WithOrigins(frontendUrl)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });

        return services;
    }

    public static IApplicationBuilder UseCorsConfiguration(this IApplicationBuilder app)
    {
        app.UseCors(AllowFrontendPolicy);
        return app;
    }
}

