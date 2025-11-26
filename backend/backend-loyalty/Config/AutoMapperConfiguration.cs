using backend_loyalty.Mappings;

namespace backend_loyalty.Config;

/// <summary>
/// AutoMapper configuration and registration
/// </summary>
public static class AutoMapperConfiguration
{
    public static IServiceCollection AddAutoMapperConfiguration(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile));
        return services;
    }
}
