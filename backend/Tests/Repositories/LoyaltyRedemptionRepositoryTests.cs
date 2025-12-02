using backend.Data;
using backend.Models;
using backend.Repositories;
using FluentAssertions;
using Xunit;

namespace Backend.Tests.Repositories;

public class LoyaltyRedemptionRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly LoyaltyRedemptionRepository _repository;

    public LoyaltyRedemptionRepositoryTests()
    {
        _context = RepositoryTestsHelper.CreateInMemoryContext();
        _repository = new LoyaltyRedemptionRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task GetByAccountIdAsync_ReturnsRedemptionsForAccount()
    {
        // Arrange
        var accountId1 = Guid.NewGuid();
        var accountId2 = Guid.NewGuid();
        var redemptions = new List<LoyaltyRedemption>
        {
            new LoyaltyRedemption { Id = Guid.NewGuid(), LoyaltyAccountId = accountId1, PointsUsed = 50, CreatedAt = DateTime.UtcNow.AddHours(-2) },
            new LoyaltyRedemption { Id = Guid.NewGuid(), LoyaltyAccountId = accountId1, PointsUsed = 30, CreatedAt = DateTime.UtcNow.AddHours(-1) },
            new LoyaltyRedemption { Id = Guid.NewGuid(), LoyaltyAccountId = accountId2, PointsUsed = 20, CreatedAt = DateTime.UtcNow }
        };
        await _context.LoyaltyRedemptions.AddRangeAsync(redemptions);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByAccountIdAsync(accountId1);

        // Assert
        var loyaltyRedemptions = result.ToList();
        loyaltyRedemptions.Should().HaveCount(2);
        loyaltyRedemptions.All(r => r.LoyaltyAccountId == accountId1).Should().BeTrue();
    }

    [Fact]
    public async Task GetByUserIdAsync_ReturnsRedemptionsForUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var account = new LoyaltyAccount { Id = accountId, UserId = userId, PointsBalance = 100, IsActive = true };
        var redemption = new LoyaltyRedemption { Id = Guid.NewGuid(), LoyaltyAccountId = accountId, PointsUsed = 50, CreatedAt = DateTime.UtcNow };
        await _context.LoyaltyAccounts.AddAsync(account);
        await _context.LoyaltyRedemptions.AddAsync(redemption);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUserIdAsync(userId);

        // Assert
        var loyaltyRedemptions = result.ToList();
        loyaltyRedemptions.Should().HaveCount(1);
        loyaltyRedemptions.First().LoyaltyAccount.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task GetByRewardTypeAsync_ReturnsRedemptionsByType()
    {
        // Arrange
        var redemptions = new List<LoyaltyRedemption>
        {
            new LoyaltyRedemption { Id = Guid.NewGuid(), LoyaltyAccountId = Guid.NewGuid(), PointsUsed = 50, RewardType = "Discount", CreatedAt = DateTime.UtcNow },
            new LoyaltyRedemption { Id = Guid.NewGuid(), LoyaltyAccountId = Guid.NewGuid(), PointsUsed = 30, RewardType = "Discount", CreatedAt = DateTime.UtcNow },
            new LoyaltyRedemption { Id = Guid.NewGuid(), LoyaltyAccountId = Guid.NewGuid(), PointsUsed = 20, RewardType = "Voucher", CreatedAt = DateTime.UtcNow }
        };
        await _context.LoyaltyRedemptions.AddRangeAsync(redemptions);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByRewardTypeAsync("Discount");

        // Assert
        var loyaltyRedemptions = result.ToList();
        loyaltyRedemptions.Should().HaveCount(2);
        loyaltyRedemptions.All(r => r.RewardType == "Discount").Should().BeTrue();
    }

    [Fact]
    public async Task GetRecentRedemptionsAsync_ReturnsLimitedRedemptions()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var redemptions = new List<LoyaltyRedemption>();
        for (int i = 0; i < 10; i++)
        {
            redemptions.Add(new LoyaltyRedemption
            {
                Id = Guid.NewGuid(),
                LoyaltyAccountId = accountId,
                PointsUsed = 10,
                CreatedAt = DateTime.UtcNow.AddHours(-i)
            });
        }
        await _context.LoyaltyRedemptions.AddRangeAsync(redemptions);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetRecentRedemptionsAsync(accountId, 5);

        // Assert
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetTotalPointsRedeemedAsync_ReturnsSumOfPoints()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var redemptions = new List<LoyaltyRedemption>
        {
            new LoyaltyRedemption { Id = Guid.NewGuid(), LoyaltyAccountId = accountId, PointsUsed = 50, CreatedAt = DateTime.UtcNow },
            new LoyaltyRedemption { Id = Guid.NewGuid(), LoyaltyAccountId = accountId, PointsUsed = 30, CreatedAt = DateTime.UtcNow },
            new LoyaltyRedemption { Id = Guid.NewGuid(), LoyaltyAccountId = Guid.NewGuid(), PointsUsed = 20, CreatedAt = DateTime.UtcNow }
        };
        await _context.LoyaltyRedemptions.AddRangeAsync(redemptions);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetTotalPointsRedeemedAsync(accountId);

        // Assert
        result.Should().Be(80);
    }
}

