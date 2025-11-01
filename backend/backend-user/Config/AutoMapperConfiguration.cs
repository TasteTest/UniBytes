using backend_user.Mappings;

namespace backend_user.Config;

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

