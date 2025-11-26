using backend_loyalty.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace backend_loyalty.Repositories.Interfaces;

/// <summary>
/// Unit of Work pattern interface
/// </summary>
public interface IUnitOfWork : IDisposable
{
    ILoyaltyAccountRepository LoyaltyAccounts { get; }
    ILoyaltyTransactionRepository LoyaltyTransactions { get; }
    ILoyaltyRedemptionRepository LoyaltyRedemptions { get; }
    ApplicationDbContext Context { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    IExecutionStrategy CreateExecutionStrategy();
}
