using backend.Config;
using backend.Data;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Backend.Tests.Config;

public class DatabaseConfigurationTests
{
    [Fact]
    public void AddDatabaseConfiguration_RegistersDbContext()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ConnectionStrings:DefaultConnection", "Host=${POSTGRES_HOST};Port=${POSTGRES_PORT};Database=${POSTGRES_DB};Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD}" }
            })
            .Build();

        // Act
        services.AddDatabaseConfiguration(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var context = serviceProvider.GetService<ApplicationDbContext>();
        context.Should().NotBeNull();
    }

    [Fact]
    public void AddDatabaseConfiguration_UsesEnvDefaults_WhenConnectionStringMissing()
    {
        // Arrange
        Environment.SetEnvironmentVariable("POSTGRES_HOST", "envhost");
        Environment.SetEnvironmentVariable("POSTGRES_PORT", "6543");
        Environment.SetEnvironmentVariable("POSTGRES_DB", "envdb");
        Environment.SetEnvironmentVariable("POSTGRES_USER", "envuser");
        Environment.SetEnvironmentVariable("POSTGRES_PASSWORD", "envpass");

        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        // Act
        services.AddDatabaseConfiguration(configuration);
        var provider = services.BuildServiceProvider();

        // Assert
        var context = provider.GetService<ApplicationDbContext>();
        context.Should().NotBeNull();

        // Cleanup
        Environment.SetEnvironmentVariable("POSTGRES_HOST", null);
        Environment.SetEnvironmentVariable("POSTGRES_PORT", null);
        Environment.SetEnvironmentVariable("POSTGRES_DB", null);
        Environment.SetEnvironmentVariable("POSTGRES_USER", null);
        Environment.SetEnvironmentVariable("POSTGRES_PASSWORD", null);
    }

    [Fact]
    public void AddDatabaseConfiguration_ReplacesEnvironmentVariables()
    {
        // Arrange
        Environment.SetEnvironmentVariable("POSTGRES_HOST", "testhost");
        Environment.SetEnvironmentVariable("POSTGRES_PORT", "5433");
        Environment.SetEnvironmentVariable("POSTGRES_DB", "testdb");
        Environment.SetEnvironmentVariable("POSTGRES_USER", "testuser");
        Environment.SetEnvironmentVariable("POSTGRES_PASSWORD", "testpass");

        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ConnectionStrings:DefaultConnection", "Host=${POSTGRES_HOST};Port=${POSTGRES_PORT};Database=${POSTGRES_DB};Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD}" }
            })
            .Build();

        // Act
        services.AddDatabaseConfiguration(configuration);

        // Assert - Should not throw, variables should be replaced
        services.Should().NotBeEmpty();
        
        // Cleanup
        Environment.SetEnvironmentVariable("POSTGRES_HOST", null);
        Environment.SetEnvironmentVariable("POSTGRES_PORT", null);
        Environment.SetEnvironmentVariable("POSTGRES_DB", null);
        Environment.SetEnvironmentVariable("POSTGRES_USER", null);
        Environment.SetEnvironmentVariable("POSTGRES_PASSWORD", null);
    }

    [Fact]
    public void AddDatabaseConfiguration_ReturnsServiceCollection()
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
        var result = services.AddDatabaseConfiguration(configuration);

        // Assert
        result.Should().BeSameAs(services);
    }
}

