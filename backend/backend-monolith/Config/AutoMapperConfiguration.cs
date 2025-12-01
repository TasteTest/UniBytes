namespace backend_monolith.Config;

/// <summary>
/// AutoMapper configuration extension methods
/// </summary>
public static class AutoMapperConfiguration
{
    public static IServiceCollection AddAutoMapperConfiguration(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(Program).Assembly);
        return services;
    }
}

