using backend.Config;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Backend.Tests.Config;

public class CorsConfigurationTests
{
    [Fact]
    public void AddCorsConfiguration_RegistersCorsPolicy()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Cors:AllowedOrigins:0", "http://localhost:3000" },
                { "Cors:AllowedOrigins:1", "http://localhost:3001" }
            })
            .Build();

        // Act
        services.AddCorsConfiguration(configuration);

        // Assert - Verify CORS services are registered
        services.Should().Contain(s => s.ServiceType == typeof(ICorsService));
    }

    [Fact]
    public void AddCorsConfiguration_UsesDefaultOrigins_WhenNotConfigured()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        // Act
        services.AddCorsConfiguration(configuration);

        // Assert - Should not throw and use default origins
        services.Should().NotBeEmpty();
    }

    [Fact]
    public void AddCorsConfiguration_ReturnsServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        // Act
        var result = services.AddCorsConfiguration(configuration);

        // Assert
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void UseCorsConfiguration_ConfiguresCorsMiddleware()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCors();
        var serviceProvider = services.BuildServiceProvider();
        var appBuilder = new ApplicationBuilder(serviceProvider);

        // Act
        var result = appBuilder.UseCorsConfiguration();

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void UseCorsConfiguration_ReturnsApplicationBuilder()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCors();
        var serviceProvider = services.BuildServiceProvider();
        var appBuilder = new ApplicationBuilder(serviceProvider);

        // Act
        var result = appBuilder.UseCorsConfiguration();

        // Assert
        result.Should().BeSameAs(appBuilder);
    }
}

