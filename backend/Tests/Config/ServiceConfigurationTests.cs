using backend.Config;
using backend.Services;
using backend.Services.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Xunit;

namespace Backend.Tests.Config;

public class ServiceConfigurationTests
{
    [Fact]
    public void AddServiceConfiguration_RegistersAllServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        // Act
        services.AddServiceConfiguration(configuration);

        // Assert - Verify services are registered by checking service descriptors
        services.Should().Contain(s => s.ServiceType == typeof(IAuthService));
        services.Should().Contain(s => s.ServiceType == typeof(IUserService));
        services.Should().Contain(s => s.ServiceType == typeof(IOAuthProviderService));
        services.Should().Contain(s => s.ServiceType == typeof(IUserAnalyticsService));
        services.Should().Contain(s => s.ServiceType == typeof(IPaymentService));
        services.Should().Contain(s => s.ServiceType == typeof(IStripeService));
        services.Should().Contain(s => s.ServiceType == typeof(IMenuService));
        services.Should().Contain(s => s.ServiceType == typeof(IBlobStorageService));
        services.Should().Contain(s => s.ServiceType == typeof(ILoyaltyAccountService));
    }

    [Fact]
    public void AddServiceConfiguration_ReturnsServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        // Act
        var result = services.AddServiceConfiguration(configuration);

        // Assert
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddServiceConfiguration_RegistersServicesAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        services.AddServiceConfiguration(configuration);

        // Assert - Verify services are registered as Scoped
        var authServiceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IAuthService));
        authServiceDescriptor.Should().NotBeNull();
        authServiceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }
}

