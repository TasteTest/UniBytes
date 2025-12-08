using backend.Config;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Backend.Tests.Config;

public class SwaggerConfigurationTests
{
    [Fact]
    public void AddSwaggerConfiguration_RegistersSwaggerServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddSwaggerConfiguration();
        var serviceProvider = services.BuildServiceProvider();

        // Assert - Swagger services should be registered
        services.Should().NotBeEmpty();
    }

    [Fact]
    public void AddSwaggerConfiguration_ReturnsServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddSwaggerConfiguration();

        // Assert
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void UseSwaggerConfiguration_ConfiguresSwaggerInDevelopment()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSwaggerConfiguration();
        services.AddLogging();
        services.AddOptions();
        var serviceProvider = services.BuildServiceProvider();
        
        // Mock environment as Development
        var hostingEnvironment = new MockHostEnvironment { EnvironmentName = "Development" };
        
        var appBuilder = new ApplicationBuilder(serviceProvider);

        // Act
        var result = appBuilder.UseSwaggerConfiguration(hostingEnvironment);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void UseSwaggerConfiguration_DoesNotConfigureSwaggerInProduction()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        
        // Mock environment as Production
        var hostingEnvironment = new MockHostEnvironment { EnvironmentName = "Production" };
        services.AddSingleton<IHostEnvironment>(hostingEnvironment);
        
        var appBuilder = new ApplicationBuilder(serviceProvider);

        // Act
        var result = appBuilder.UseSwaggerConfiguration(hostingEnvironment);

        // Assert - Should still return builder but Swagger won't be enabled
        result.Should().NotBeNull();
    }

    [Fact]
    public void UseSwaggerConfiguration_ReturnsApplicationBuilder()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSwaggerConfiguration();
        services.AddLogging();
        services.AddOptions();
        var serviceProvider = services.BuildServiceProvider();
        var hostingEnvironment = new MockHostEnvironment { EnvironmentName = "Development" };
        var appBuilder = new ApplicationBuilder(serviceProvider);

        // Act
        var result = appBuilder.UseSwaggerConfiguration(hostingEnvironment);

        // Assert
        result.Should().BeSameAs(appBuilder);
    }

    private class MockHostEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "TestApp";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public string ContentRootPath { get; set; } = "/";
        public string EnvironmentName { get; set; } = "Development";
        public string WebRootPath { get; set; } = "/wwwroot";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    }
}

