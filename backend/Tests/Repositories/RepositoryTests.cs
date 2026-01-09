using backend.Data;
using backend.Models;
using backend.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Backend.Tests.Repositories;

public class RepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Repository<User> _repository;

    public RepositoryTests()
    {
        _context = RepositoryTestsHelper.CreateInMemoryContext();
        _repository = new Repository<User>(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_EntityExists_ReturnsEntity()
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
        var result = await _repository.GetByIdAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Email.Should().Be(user.Email);
    }

    [Fact]
    public async Task GetByIdAsync_EntityNotExists_ReturnsNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ReturnsAllEntities()
    {
        // Arrange
        var users = new List<User>
        {
            new User { Id = Guid.NewGuid(), Email = "user1@example.com", FirstName = "User", LastName = "One" },
            new User { Id = Guid.NewGuid(), Email = "user2@example.com", FirstName = "User", LastName = "Two" }
        };
        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        var enumerable = result.ToList();
        enumerable.Should().HaveCount(2);
        enumerable.Should().Contain(u => u.Email == "user1@example.com");
        enumerable.Should().Contain(u => u.Email == "user2@example.com");
    }

    [Fact]
    public async Task GetAllAsync_NoEntities_ReturnsEmpty()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region FindAsync Tests

    [Fact]
    public async Task FindAsync_WithPredicate_ReturnsMatchingEntities()
    {
        // Arrange
        var users = new List<User>
        {
            new User { Id = Guid.NewGuid(), Email = "active@example.com", FirstName = "Active", LastName = "User", IsActive = true },
            new User { Id = Guid.NewGuid(), Email = "inactive@example.com", FirstName = "Inactive", LastName = "User", IsActive = false }
        };
        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.FindAsync(u => u.IsActive);

        // Assert
        var enumerable = result.ToList();
        enumerable.Should().HaveCount(1);
        enumerable.First().Email.Should().Be("active@example.com");
    }

    [Fact]
    public async Task FindAsync_NoMatches_ReturnsEmpty()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "user@example.com", IsActive = true };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.FindAsync(u => u.Role == backend.Common.Enums.UserRole.Admin);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region FirstOrDefaultAsync Tests

    [Fact]
    public async Task FirstOrDefaultAsync_EntityExists_ReturnsFirstMatching()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "test@example.com", FirstName = "Test" };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.FirstOrDefaultAsync(u => u.Email == "test@example.com");

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task FirstOrDefaultAsync_EntityNotExists_ReturnsNull()
    {
        // Act
        var result = await _repository.FirstOrDefaultAsync(u => u.Email == "nonexistent@example.com");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region AnyAsync Tests

    [Fact]
    public async Task AnyAsync_EntityExists_ReturnsTrue()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "test@example.com" };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.AnyAsync(u => u.Email == "test@example.com");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task AnyAsync_EntityNotExists_ReturnsFalse()
    {
        // Act
        var result = await _repository.AnyAsync(u => u.Email == "nonexistent@example.com");

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region CountAsync Tests

    [Fact]
    public async Task CountAsync_WithoutPredicate_ReturnsTotalCount()
    {
        // Arrange
        var users = new List<User>
        {
            new User { Id = Guid.NewGuid(), Email = "user1@example.com" },
            new User { Id = Guid.NewGuid(), Email = "user2@example.com" },
            new User { Id = Guid.NewGuid(), Email = "user3@example.com" }
        };
        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.CountAsync();

        // Assert
        result.Should().Be(3);
    }

    [Fact]
    public async Task CountAsync_WithPredicate_ReturnsMatchingCount()
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
        var result = await _repository.CountAsync(u => u.IsActive);

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public async Task CountAsync_NoEntities_ReturnsZero()
    {
        // Act
        var result = await _repository.CountAsync();

        // Assert
        result.Should().Be(0);
    }

    #endregion

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_AddsEntityToDatabase()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "newuser@example.com",
            FirstName = "New",
            LastName = "User"
        };

        // Act
        var result = await _repository.AddAsync(user);

        // Assert
        result.Should().BeSameAs(user);
        var savedUser = await _context.Users.FindAsync(user.Id);
        savedUser.Should().NotBeNull();
        savedUser!.Email.Should().Be(user.Email);
    }

    #endregion

    #region AddRangeAsync Tests

    [Fact]
    public async Task AddRangeAsync_AddsMultipleEntities()
    {
        // Arrange
        var users = new List<User>
        {
            new User { Id = Guid.NewGuid(), Email = "user1@example.com" },
            new User { Id = Guid.NewGuid(), Email = "user2@example.com" }
        };

        // Act
        await _repository.AddRangeAsync(users);

        // Assert
        var count = await _context.Users.CountAsync();
        count.Should().Be(2);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_UpdatesEntity()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "test@example.com", FirstName = "Old" };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        user.FirstName = "New";
        await _repository.UpdateAsync(user);

        // Assert
        var updatedUser = await _context.Users.FindAsync(user.Id);
        updatedUser!.FirstName.Should().Be("New");
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_DeletesEntity()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "test@example.com" };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(user);

        // Assert
        var deletedUser = await _context.Users.FindAsync(user.Id);
        deletedUser.Should().BeNull();
    }

    #endregion

    #region DeleteRangeAsync Tests

    [Fact]
    public async Task DeleteRangeAsync_DeletesMultipleEntities()
    {
        // Arrange
        var users = new List<User>
        {
            new User { Id = Guid.NewGuid(), Email = "user1@example.com" },
            new User { Id = Guid.NewGuid(), Email = "user2@example.com" }
        };
        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteRangeAsync(users);

        // Assert
        var count = await _context.Users.CountAsync();
        count.Should().Be(0);
    }

    #endregion
}

