using Microsoft.OpenApi.Models;

namespace backend_loyalty.Config;

/// <summary>
/// Swagger/OpenAPI configuration
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
                Title = "Loyalty Service API",
                Version = "v1",
                Description = "API for managing loyalty accounts, points, and redemptions",
                Contact = new OpenApiContact
                {
                    Name = "Teo",
                    Email = "teo@campuseats.com"
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
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Loyalty Service API v1");
            options.RoutePrefix = "swagger";
        });

        return app;
    }
}
