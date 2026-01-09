using backend.Common.Enums;
using backend.Data;
using backend.Models;
using backend.Repositories;
using FluentAssertions;
using Xunit;

namespace Backend.Tests.Repositories;

public class UserRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        _context = RepositoryTestsHelper.CreateInMemoryContext();
        _repository = new UserRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task GetByEmailAsync_UserExists_ReturnsUser()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByEmailAsync("test@example.com");

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task GetByEmailAsync_UserNotExists_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByEmailAsync("nonexistent@example.com");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdWithOAuthProvidersAsync_UserWithProviders_ReturnsUserWithProviders()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Email = "test@example.com" };
        var provider = new OAuthProvider
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Provider = OAuthProviderType.Google,
            ProviderId = "google123"
        };
        await _context.Users.AddAsync(user);
        await _context.OAuthProviders.AddAsync(provider);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdWithOAuthProvidersAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.OAuthProviders.Should().HaveCount(1);
        result.OAuthProviders.First().Provider.Should().Be(OAuthProviderType.Google);
    }

    [Fact]
    public async Task GetActiveUsersAsync_ReturnsOnlyActiveUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new User { Id = Guid.NewGuid(), Email = "active1@example.com", IsActive = true },
            new User { Id = Guid.NewGuid(), Email = "active2@example.com", IsActive = true },
            new User { Id = Guid.NewGuid(), Email = "inactive@example.com", IsActive = false }
        };
        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveUsersAsync();

        // Assert
        var enumerable = result.ToList();
        enumerable.Should().HaveCount(2);
        enumerable.All(u => u.IsActive).Should().BeTrue();
    }

    [Fact]
    public async Task GetAdminUsersAsync_ReturnsOnlyAdminUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new User { Id = Guid.NewGuid(), Email = "admin1@example.com", Role = UserRole.Admin, IsActive = true },
            new User { Id = Guid.NewGuid(), Email = "admin2@example.com", Role = UserRole.Admin, IsActive = true },
            new User { Id = Guid.NewGuid(), Email = "user@example.com", Role = UserRole.User, IsActive = true },
            new User { Id = Guid.NewGuid(), Email = "inactiveadmin@example.com", Role = UserRole.Admin, IsActive = false }
        };
        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAdminUsersAsync();

        // Assert
        var enumerable = result.ToList();
        enumerable.Should().HaveCount(2);
        enumerable.All(u => u is { Role: UserRole.Admin, IsActive: true }).Should().BeTrue();
    }

    [Fact]
    public async Task EmailExistsAsync_EmailExists_ReturnsTrue()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "exists@example.com" };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.EmailExistsAsync("exists@example.com");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task EmailExistsAsync_EmailNotExists_ReturnsFalse()
    {
        // Act
        var result = await _repository.EmailExistsAsync("nonexistent@example.com");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateLastLoginAsync_UserExists_UpdatesLastLogin()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "test@example.com" };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        var beforeUpdate = user.LastLoginAt;

        // Act
        await _repository.UpdateLastLoginAsync(user.Id);
        await _context.SaveChangesAsync();

        // Assert
        var updatedUser = await _context.Users.FindAsync(user.Id);
        updatedUser!.LastLoginAt.Should().NotBeNull();
        updatedUser.LastLoginAt.Should().BeAfter(beforeUpdate ?? DateTime.MinValue);
    }

    [Fact]
    public async Task UpdateLastLoginAsync_UserNotExists_DoesNotThrow()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var act = async () => await _repository.UpdateLastLoginAsync(nonExistentId);

        // Assert
        await act.Should().NotThrowAsync();
    }
}

