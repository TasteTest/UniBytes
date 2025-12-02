using backend.Data;
using backend.Modelss;
using backend.Repositories;
using FluentAssertions;
using Xunit;

namespace Backend.Tests.Repositories;

public class IdempotencyKeyRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IdempotencyKeyRepository _repository;

    public IdempotencyKeyRepositoryTests()
    {
        _context = RepositoryTestsHelper.CreateInMemoryContext();
        _repository = new IdempotencyKeyRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task GetByKeyAsync_KeyExists_ReturnsIdempotencyKey()
    {
        // Arrange
        var key = "test-key-123";
        var idempotencyKey = new IdempotencyKey
        {
            Id = Guid.NewGuid(),
            Key = key,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(1)
        };
        await _context.IdempotencyKeys.AddAsync(idempotencyKey);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByKeyAsync(key);

        // Assert
        result.Should().NotBeNull();
        result!.Key.Should().Be(key);
    }

    [Fact]
    public async Task GetByKeyAsync_KeyNotExists_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByKeyAsync("nonexistent-key");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task KeyExistsAsync_KeyExistsAndNotExpired_ReturnsTrue()
    {
        // Arrange
        var key = "valid-key";
        var idempotencyKey = new IdempotencyKey
        {
            Id = Guid.NewGuid(),
            Key = key,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(1)
        };
        await _context.IdempotencyKeys.AddAsync(idempotencyKey);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.KeyExistsAsync(key);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task KeyExistsAsync_KeyExistsButExpired_ReturnsFalse()
    {
        // Arrange
        var key = "expired-key";
        var idempotencyKey = new IdempotencyKey
        {
            Id = Guid.NewGuid(),
            Key = key,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(-1)
        };
        await _context.IdempotencyKeys.AddAsync(idempotencyKey);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.KeyExistsAsync(key);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task KeyExistsAsync_KeyWithNoExpiration_ReturnsTrue()
    {
        // Arrange
        var key = "no-expiry-key";
        var idempotencyKey = new IdempotencyKey
        {
            Id = Guid.NewGuid(),
            Key = key,
            ExpiresAt = null
        };
        await _context.IdempotencyKeys.AddAsync(idempotencyKey);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.KeyExistsAsync(key);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task KeyExistsAsync_KeyNotExists_ReturnsFalse()
    {
        // Act
        var result = await _repository.KeyExistsAsync("nonexistent-key");

        // Assert
        result.Should().BeFalse();
    }
}

