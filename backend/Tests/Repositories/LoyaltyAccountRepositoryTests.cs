using backend.Common.Enums;
using backend.Data;
using backend.Models;
using backend.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Backend.Tests.Repositories;

public class LoyaltyAccountRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly LoyaltyAccountRepository _repository;

    public LoyaltyAccountRepositoryTests()
    {
        _context = RepositoryTestsHelper.CreateInMemoryContext();
        _repository = new LoyaltyAccountRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task GetByUserIdAsync_AccountExists_ReturnsAccount()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var account = new LoyaltyAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PointsBalance = 100,
            Tier = LoyaltyTier.Bronze,
            IsActive = true
        };
        await _context.LoyaltyAccounts.AddAsync(account);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUserIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task GetByUserIdWithTransactionsAsync_ReturnsAccountWithTransactions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var account = new LoyaltyAccount
        {
            Id = accountId,
            UserId = userId,
            PointsBalance = 100,
            IsActive = true
        };
        var transaction = new LoyaltyTransaction
        {
            Id = Guid.NewGuid(),
            LoyaltyAccountId = accountId,
            ChangeAmount = 50,
            Reason = "Purchase",
            CreatedAt = DateTime.UtcNow
        };
        await _context.LoyaltyAccounts.AddAsync(account);
        await _context.LoyaltyTransactions.AddAsync(transaction);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUserIdWithTransactionsAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.LoyaltyTransactions.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByUserIdWithRedemptionsAsync_ReturnsAccountWithRedemptions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var account = new LoyaltyAccount { Id = accountId, UserId = userId, PointsBalance = 100, IsActive = true };
        var redemption = new LoyaltyRedemption
        {
            Id = Guid.NewGuid(),
            LoyaltyAccountId = accountId,
            PointsUsed = 50,
            RewardType = "Discount",
            CreatedAt = DateTime.UtcNow
        };
        await _context.LoyaltyAccounts.AddAsync(account);
        await _context.LoyaltyRedemptions.AddAsync(redemption);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUserIdWithRedemptionsAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.LoyaltyRedemptions.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByUserIdWithAllDataAsync_ReturnsAccountWithAllData()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var account = new LoyaltyAccount { Id = accountId, UserId = userId, PointsBalance = 100, IsActive = true };
        var transaction = new LoyaltyTransaction { Id = Guid.NewGuid(), LoyaltyAccountId = accountId, ChangeAmount = 50, CreatedAt = DateTime.UtcNow };
        var redemption = new LoyaltyRedemption { Id = Guid.NewGuid(), LoyaltyAccountId = accountId, PointsUsed = 50, CreatedAt = DateTime.UtcNow };
        await _context.LoyaltyAccounts.AddAsync(account);
        await _context.LoyaltyTransactions.AddAsync(transaction);
        await _context.LoyaltyRedemptions.AddAsync(redemption);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUserIdWithAllDataAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.LoyaltyTransactions.Should().NotBeNull();
        result.LoyaltyRedemptions.Should().NotBeNull();
    }

    [Fact]
    public async Task GetActiveAccountsAsync_ReturnsOnlyActiveAccounts()
    {
        // Arrange
        var accounts = new List<LoyaltyAccount>
        {
            new LoyaltyAccount { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), PointsBalance = 100, IsActive = true },
            new LoyaltyAccount { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), PointsBalance = 200, IsActive = true },
            new LoyaltyAccount { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), PointsBalance = 300, IsActive = false }
        };
        await _context.LoyaltyAccounts.AddRangeAsync(accounts);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveAccountsAsync();

        // Assert
        var loyaltyAccounts = result.ToList();
        loyaltyAccounts.Should().HaveCount(2);
        loyaltyAccounts.All(a => a.IsActive).Should().BeTrue();
    }

    [Fact]
    public async Task GetByTierAsync_ReturnsAccountsForTier()
    {
        // Arrange
        var accounts = new List<LoyaltyAccount>
        {
            new LoyaltyAccount { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), PointsBalance = 100, Tier = LoyaltyTier.Bronze, IsActive = true },
            new LoyaltyAccount { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), PointsBalance = 200, Tier = LoyaltyTier.Bronze, IsActive = true },
            new LoyaltyAccount { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), PointsBalance = 500, Tier = LoyaltyTier.Silver, IsActive = true }
        };
        await _context.LoyaltyAccounts.AddRangeAsync(accounts);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByTierAsync((int)LoyaltyTier.Bronze);

        // Assert
        var loyaltyAccounts = result.ToList();
        loyaltyAccounts.Should().HaveCount(2);
        loyaltyAccounts.All(a => (int)a.Tier == (int)LoyaltyTier.Bronze && a.IsActive).Should().BeTrue();
    }

    [Fact]
    public async Task UserHasAccountAsync_AccountExists_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var account = new LoyaltyAccount { Id = Guid.NewGuid(), UserId = userId, PointsBalance = 100, IsActive = true };
        await _context.LoyaltyAccounts.AddAsync(account);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.UserHasAccountAsync(userId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task UserHasAccountAsync_AccountNotExists_ReturnsFalse()
    {
        // Act
        var result = await _repository.UserHasAccountAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddPointsAsync_AccountExists_AddsPointsAndTransaction()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var account = new LoyaltyAccount { Id = Guid.NewGuid(), UserId = userId, PointsBalance = 100, IsActive = true };
        await _context.LoyaltyAccounts.AddAsync(account);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.AddPointsAsync(userId, 50, "Purchase");
        await _context.SaveChangesAsync();

        // Assert
        result.Should().BeTrue();
        var updatedAccount = await _context.LoyaltyAccounts.FindAsync(account.Id);
        updatedAccount!.PointsBalance.Should().Be(150);
        var transaction = await _context.LoyaltyTransactions.FirstOrDefaultAsync(t => t.LoyaltyAccountId == account.Id);
        transaction.Should().NotBeNull();
        transaction!.ChangeAmount.Should().Be(50);
    }

    [Fact]
    public async Task AddPointsAsync_AccountNotExists_ReturnsFalse()
    {
        // Act
        var result = await _repository.AddPointsAsync(Guid.NewGuid(), 50, "Purchase");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeductPointsAsync_AccountExistsAndHasEnoughPoints_DeductsPoints()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var account = new LoyaltyAccount { Id = Guid.NewGuid(), UserId = userId, PointsBalance = 100, IsActive = true };
        await _context.LoyaltyAccounts.AddAsync(account);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeductPointsAsync(userId, 50, "Redemption");
        await _context.SaveChangesAsync();

        // Assert
        result.Should().BeTrue();
        var updatedAccount = await _context.LoyaltyAccounts.FindAsync(account.Id);
        updatedAccount!.PointsBalance.Should().Be(50);
    }

    [Fact]
    public async Task DeductPointsAsync_AccountNotExists_ReturnsFalse()
    {
        // Act
        var result = await _repository.DeductPointsAsync(Guid.NewGuid(), 50, "Redemption");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeductPointsAsync_InsufficientPoints_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var account = new LoyaltyAccount { Id = Guid.NewGuid(), UserId = userId, PointsBalance = 30, IsActive = true };
        await _context.LoyaltyAccounts.AddAsync(account);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeductPointsAsync(userId, 50, "Redemption");

        // Assert
        result.Should().BeFalse();
        var unchangedAccount = await _context.LoyaltyAccounts.FindAsync(account.Id);
        unchangedAccount!.PointsBalance.Should().Be(30);
    }
}

