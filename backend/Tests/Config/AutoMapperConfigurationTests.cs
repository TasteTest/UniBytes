using backend.Config;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Backend.Tests.Config;

public class AutoMapperConfigurationTests
{
    [Fact]
    public void AddAutoMapperConfiguration_RegistersAutoMapper()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAutoMapperConfiguration();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var mapper = serviceProvider.GetService<AutoMapper.IMapper>();
        mapper.Should().NotBeNull();
    }

    [Fact]
    public void AddAutoMapperConfiguration_ReturnsServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddAutoMapperConfiguration();

        // Assert
        result.Should().BeSameAs(services);
    }
}

