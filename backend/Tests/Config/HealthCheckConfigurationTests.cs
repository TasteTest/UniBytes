using backend.Config;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Xunit;

namespace Backend.Tests.Config;

public class HealthCheckConfigurationTests
{
    [Fact]
    public void AddHealthCheckConfiguration_RegistersHealthChecks()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ConnectionStrings:DefaultConnection", "Host=localhost;Database=test" }
            })
            .Build();

        // Act
        services.AddHealthCheckConfiguration(configuration);

        // Assert - Verify health check services are registered
        services.Should().Contain(s => s.ServiceType == typeof(HealthCheckService));
    }

    [Fact]
    public void AddHealthCheckConfiguration_ReturnsServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ConnectionStrings:DefaultConnection", "Host=localhost;Database=test" }
            })
            .Build();

        // Act
        var result = services.AddHealthCheckConfiguration(configuration);

        // Assert
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void UseHealthCheckConfiguration_ConfiguresHealthCheckEndpoint()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOptions();
        services.AddHealthChecks();
        var serviceProvider = services.BuildServiceProvider();
        var appBuilder = new ApplicationBuilder(serviceProvider);

        // Act
        var result = appBuilder.UseHealthCheckConfiguration();

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void UseHealthCheckConfiguration_ReturnsApplicationBuilder()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOptions();
        services.AddHealthChecks();
        var serviceProvider = services.BuildServiceProvider();
        var appBuilder = new ApplicationBuilder(serviceProvider);

        // Act
        var result = appBuilder.UseHealthCheckConfiguration();

        // Assert
        result.Should().BeSameAs(appBuilder);
    }
}

