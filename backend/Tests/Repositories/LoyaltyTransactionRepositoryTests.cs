using backend.Data;
using backend.Models;
using backend.Repositories;
using FluentAssertions;
using Xunit;

namespace Backend.Tests.Repositories;

public class LoyaltyTransactionRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly LoyaltyTransactionRepository _repository;

    public LoyaltyTransactionRepositoryTests()
    {
        _context = RepositoryTestsHelper.CreateInMemoryContext();
        _repository = new LoyaltyTransactionRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task GetByAccountIdAsync_ReturnsTransactionsForAccount()
    {
        // Arrange
        var accountId1 = Guid.NewGuid();
        var accountId2 = Guid.NewGuid();
        var transactions = new List<LoyaltyTransaction>
        {
            new LoyaltyTransaction { Id = Guid.NewGuid(), LoyaltyAccountId = accountId1, ChangeAmount = 50, CreatedAt = DateTime.UtcNow.AddHours(-2) },
            new LoyaltyTransaction { Id = Guid.NewGuid(), LoyaltyAccountId = accountId1, ChangeAmount = 30, CreatedAt = DateTime.UtcNow.AddHours(-1) },
            new LoyaltyTransaction { Id = Guid.NewGuid(), LoyaltyAccountId = accountId2, ChangeAmount = 20, CreatedAt = DateTime.UtcNow }
        };
        await _context.LoyaltyTransactions.AddRangeAsync(transactions);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByAccountIdAsync(accountId1);

        // Assert
        var loyaltyTransactions = result.ToList();
        loyaltyTransactions.Should().HaveCount(2);
        loyaltyTransactions.All(t => t.LoyaltyAccountId == accountId1).Should().BeTrue();
        loyaltyTransactions.First().CreatedAt.Should().BeAfter(loyaltyTransactions.Last().CreatedAt); // Ordered by descending
    }

    [Fact]
    public async Task GetByUserIdAsync_ReturnsTransactionsForUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var account = new LoyaltyAccount { Id = accountId, UserId = userId, PointsBalance = 100, IsActive = true };
        var transaction = new LoyaltyTransaction { Id = Guid.NewGuid(), LoyaltyAccountId = accountId, ChangeAmount = 50, CreatedAt = DateTime.UtcNow };
        await _context.LoyaltyAccounts.AddAsync(account);
        await _context.LoyaltyTransactions.AddAsync(transaction);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUserIdAsync(userId);

        // Assert
        var loyaltyTransactions = result.ToList();
        loyaltyTransactions.Should().HaveCount(1);
        loyaltyTransactions.First().LoyaltyAccount.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task GetByReferenceIdAsync_ReturnsTransactionsWithReference()
    {
        // Arrange
        var referenceId = Guid.NewGuid();
        var transactions = new List<LoyaltyTransaction>
        {
            new LoyaltyTransaction { Id = Guid.NewGuid(), LoyaltyAccountId = Guid.NewGuid(), ChangeAmount = 50, ReferenceId = referenceId, CreatedAt = DateTime.UtcNow.AddHours(-1) },
            new LoyaltyTransaction { Id = Guid.NewGuid(), LoyaltyAccountId = Guid.NewGuid(), ChangeAmount = 30, ReferenceId = referenceId, CreatedAt = DateTime.UtcNow },
            new LoyaltyTransaction { Id = Guid.NewGuid(), LoyaltyAccountId = Guid.NewGuid(), ChangeAmount = 20, ReferenceId = null, CreatedAt = DateTime.UtcNow }
        };
        await _context.LoyaltyTransactions.AddRangeAsync(transactions);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByReferenceIdAsync(referenceId);

        // Assert
        var loyaltyTransactions = result.ToList();
        loyaltyTransactions.Should().HaveCount(2);
        loyaltyTransactions.All(t => t.ReferenceId == referenceId).Should().BeTrue();
    }

    [Fact]
    public async Task GetRecentTransactionsAsync_ReturnsLimitedTransactions()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var transactions = new List<LoyaltyTransaction>();
        for (int i = 0; i < 10; i++)
        {
            transactions.Add(new LoyaltyTransaction
            {
                Id = Guid.NewGuid(),
                LoyaltyAccountId = accountId,
                ChangeAmount = 10,
                CreatedAt = DateTime.UtcNow.AddHours(-i)
            });
        }
        await _context.LoyaltyTransactions.AddRangeAsync(transactions);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetRecentTransactionsAsync(accountId, 5);

        // Assert
        var loyaltyTransactions = result.ToList();
        loyaltyTransactions.Should().HaveCount(5);
        loyaltyTransactions.First().CreatedAt.Should().BeAfter(loyaltyTransactions.Last().CreatedAt); // Most recent first
    }
}

