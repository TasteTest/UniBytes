using System;
using System.Linq;
using System.Threading.Tasks;
using backend_loyalty.Common.Enums;
using backend_loyalty.Data;
using backend_loyalty.Model;
using backend_loyalty.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Loyalty.Tests.Repositories;

public class LoyaltyAccountRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly LoyaltyAccountRepository _repository;

    public LoyaltyAccountRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new LoyaltyAccountRepository(_context);
    }

    [Fact]
    public async Task GetByUserIdAsync_WhenExists_ReturnsAccount()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var account = new LoyaltyAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PointsBalance = 100,
            Tier = LoyaltyTier.Bronze,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.LoyaltyAccounts.Add(account);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUserIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(userId);
        result.PointsBalance.Should().Be(100);
    }

    [Fact]
    public async Task GetByUserIdAsync_WhenNotExists_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByUserIdAsync(userId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_CreatesNewAccount()
    {
        // Arrange
        var account = new LoyaltyAccount
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            PointsBalance = 0,
            Tier = LoyaltyTier.Bronze,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _repository.AddAsync(account);

        // Assert
        var saved = await _context.LoyaltyAccounts.FindAsync(account.Id);
        saved.Should().NotBeNull();
        saved!.UserId.Should().Be(account.UserId);
        saved.PointsBalance.Should().Be(0);
    }

    [Fact]
    public async Task GetActiveAccountsAsync_ReturnsOnlyActiveAccounts()
    {
        // Arrange
        var activeAccount = new LoyaltyAccount
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            PointsBalance = 100,
            Tier = LoyaltyTier.Bronze,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var inactiveAccount = new LoyaltyAccount
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            PointsBalance = 50,
            Tier = LoyaltyTier.Bronze,
            IsActive = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.LoyaltyAccounts.AddRange(activeAccount, inactiveAccount);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveAccountsAsync();

        // Assert
        result.Should().HaveCount(1);
        result.Should().OnlyContain(a => a.IsActive);
        result.First().Id.Should().Be(activeAccount.Id);
    }

    [Fact]
    public async Task AddPointsAsync_IncreasesBalanceAndCreatesTransaction()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var account = new LoyaltyAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PointsBalance = 100,
            Tier = LoyaltyTier.Bronze,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.LoyaltyAccounts.Add(account);
        await _context.SaveChangesAsync();

        // Act
        var success = await _repository.AddPointsAsync(userId, 50, "Test points", null);
        await _context.SaveChangesAsync();

        // Assert
        success.Should().BeTrue();
        var updatedAccount = await _context.LoyaltyAccounts.FindAsync(account.Id);
        updatedAccount!.PointsBalance.Should().Be(150);

        var transaction = _context.Set<LoyaltyTransaction>()
            .First(t => t.LoyaltyAccountId == account.Id);
        transaction.ChangeAmount.Should().Be(50);
        transaction.Reason.Should().Be("Test points");
    }

    [Fact]
    public async Task DeductPointsAsync_DecreasesBalanceAndCreatesTransaction()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var account = new LoyaltyAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PointsBalance = 100,
            Tier = LoyaltyTier.Bronze,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.LoyaltyAccounts.Add(account);
        await _context.SaveChangesAsync();

        // Act
        var success = await _repository.DeductPointsAsync(userId, 30, "Redemption", null);
        await _context.SaveChangesAsync();

        // Assert
        success.Should().BeTrue();
        var updatedAccount = await _context.LoyaltyAccounts.FindAsync(account.Id);
        updatedAccount!.PointsBalance.Should().Be(70);

        var transaction = _context.Set<LoyaltyTransaction>()
            .First(t => t.LoyaltyAccountId == account.Id);
        transaction.ChangeAmount.Should().Be(-30);
        transaction.Reason.Should().Be("Redemption");
    }

    [Fact]
    public async Task DeductPointsAsync_WithInsufficientBalance_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var account = new LoyaltyAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PointsBalance = 20,
            Tier = LoyaltyTier.Bronze,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.LoyaltyAccounts.Add(account);
        await _context.SaveChangesAsync();

        // Act
        var success = await _repository.DeductPointsAsync(userId, 50, "Redemption", null);

        // Assert
        success.Should().BeFalse();
        var unchangedAccount = await _context.LoyaltyAccounts.FindAsync(account.Id);
        unchangedAccount!.PointsBalance.Should().Be(20);
    }

    [Fact]
    public async Task UserHasAccountAsync_WhenExists_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var account = new LoyaltyAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PointsBalance = 0,
            Tier = LoyaltyTier.Bronze,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.LoyaltyAccounts.Add(account);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.UserHasAccountAsync(userId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task UserHasAccountAsync_WhenNotExists_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var result = await _repository.UserHasAccountAsync(userId);

        // Assert
        result.Should().BeFalse();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
