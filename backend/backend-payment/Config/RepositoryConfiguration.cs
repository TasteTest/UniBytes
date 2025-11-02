using backend_payment.Data;
using backend_payment.Repositories;
using backend_payment.Repositories.Interfaces;

namespace backend_payment.Config;

/// <summary>
/// Repository configuration extension methods
/// </summary>
public static class RepositoryConfiguration
{
    public static IServiceCollection AddRepositoryConfiguration(this IServiceCollection services)
    {
        services.AddScoped<PaymentDbContext>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IIdempotencyKeyRepository, IdempotencyKeyRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        return services;
    }
}

