using backend.Config;
using backend.Modelss;
using backend.Repositories;
using backend.Repositories.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Backend.Tests.Config;

public class RepositoryConfigurationTests
{
    [Fact]
    public void AddRepositoryConfiguration_RegistersAllRepositories()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddRepositoryConfiguration();

        // Assert - Verify repositories are registered by checking service descriptors
        services.Should().Contain(s => s.ServiceType == typeof(IUserRepository));
        services.Should().Contain(s => s.ServiceType == typeof(IOAuthProviderRepository));
        services.Should().Contain(s => s.ServiceType == typeof(IUserAnalyticsRepository));
        services.Should().Contain(s => s.ServiceType == typeof(IPaymentRepository));
        services.Should().Contain(s => s.ServiceType == typeof(IIdempotencyKeyRepository));
        services.Should().Contain(s => s.ServiceType == typeof(IMenuItemRepository));
        services.Should().Contain(s => s.ServiceType == typeof(ICategoryRepository));
        services.Should().Contain(s => s.ServiceType == typeof(ILoyaltyAccountRepository));
        services.Should().Contain(s => s.ServiceType == typeof(ILoyaltyTransactionRepository));
        services.Should().Contain(s => s.ServiceType == typeof(ILoyaltyRedemptionRepository));
    }

    [Fact]
    public void AddRepositoryConfiguration_RegistersGenericRepository()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddRepositoryConfiguration();

        // Assert - Verify generic repository is registered
        // Check that IRepository<> is registered as an open generic
        services.Should().Contain(s => 
            s.ServiceType.IsGenericType && 
            s.ServiceType.GetGenericTypeDefinition() == typeof(IRepository<>));
    }

    [Fact]
    public void AddRepositoryConfiguration_ReturnsServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddRepositoryConfiguration();

        // Assert
        result.Should().BeSameAs(services);
    }
}

