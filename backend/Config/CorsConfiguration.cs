namespace backend.Config;

/// <summary>
/// CORS configuration extension methods
/// </summary>
public static class CorsConfiguration
{
    public static IServiceCollection AddCorsConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        // For testing allow any origin. Remove or restrict this in production.
        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", builder =>
            {
                builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
                // Note: AllowCredentials cannot be used with AllowAnyOrigin()
            });
        });

        return services;
    }

    public static IApplicationBuilder UseCorsConfiguration(this IApplicationBuilder app)
    {
        app.UseCors("CorsPolicy");
        return app;
    }
}

