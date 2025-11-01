using Microsoft.OpenApi.Models;

namespace backend_user.Config;

/// <summary>
/// Swagger/OpenAPI configuration and registration
/// </summary>
public static class SwaggerConfiguration
{
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "User Service API",
                Version = "v1",
                Description = "User management service with OAuth providers and analytics",
                Contact = new OpenApiContact
                {
                    Name = "Development Team",
                    Email = "dev@example.com"
                }
            });

            // Include XML comments if available
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }
        });

        return services;
    }

    public static IApplicationBuilder UseSwaggerConfiguration(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "User Service API v1");
            options.RoutePrefix = string.Empty; // Set Swagger UI at app's root
        });

        return app;
    }
}

