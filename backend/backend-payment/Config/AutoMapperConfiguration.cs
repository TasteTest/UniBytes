using backend_payment.Mappings;

namespace backend_payment.Config;

/// <summary>
/// AutoMapper configuration extension methods
/// </summary>
public static class AutoMapperConfiguration
{
    public static IServiceCollection AddAutoMapperConfiguration(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile));
        return services;
    }
}

