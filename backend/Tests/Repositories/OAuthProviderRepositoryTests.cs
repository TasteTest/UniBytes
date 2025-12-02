using backend.Common.Enums;
using backend.Data;
using backend.Modelss;
using backend.Repositories;
using FluentAssertions;
using Xunit;

namespace Backend.Tests.Repositories;

public class OAuthProviderRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly OAuthProviderRepository _repository;

    public OAuthProviderRepositoryTests()
    {
        _context = RepositoryTestsHelper.CreateInMemoryContext();
        _repository = new OAuthProviderRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task GetByUserIdAsync_ReturnsAllProvidersForUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var providers = new List<OAuthProvider>
        {
            new OAuthProvider
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Provider = OAuthProviderType.Google,
                ProviderId = "google123"
            },
            new OAuthProvider
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Provider = OAuthProviderType.Facebook,
                ProviderId = "facebook456"
            },
            new OAuthProvider
            {
                Id = Guid.NewGuid(),
                UserId = otherUserId,
                Provider = OAuthProviderType.Google,
                ProviderId = "google789"
            }
        };
        await _context.OAuthProviders.AddRangeAsync(providers);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUserIdAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result.All(p => p.UserId == userId).Should().BeTrue();
    }

    [Fact]
    public async Task GetByProviderAndProviderIdAsync_ProviderExists_ReturnsProvider()
    {
        // Arrange
        var provider = new OAuthProvider
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Provider = OAuthProviderType.Google,
            ProviderId = "google123"
        };
        await _context.OAuthProviders.AddAsync(provider);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByProviderAndProviderIdAsync(OAuthProviderType.Google, "google123");

        // Assert
        result.Should().NotBeNull();
        result!.Provider.Should().Be(OAuthProviderType.Google);
        result.ProviderId.Should().Be("google123");
    }

    [Fact]
    public async Task GetByProviderAndProviderIdAsync_ProviderNotExists_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByProviderAndProviderIdAsync(OAuthProviderType.Google, "nonexistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ExistsAsync_ProviderExists_ReturnsTrue()
    {
        // Arrange
        var provider = new OAuthProvider
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Provider = OAuthProviderType.Google,
            ProviderId = "google123"
        };
        await _context.OAuthProviders.AddAsync(provider);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsAsync(OAuthProviderType.Google, "google123");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_ProviderNotExists_ReturnsFalse()
    {
        // Act
        var result = await _repository.ExistsAsync(OAuthProviderType.Google, "nonexistent");

        // Assert
        result.Should().BeFalse();
    }
}

