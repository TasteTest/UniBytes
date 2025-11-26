using backend_loyalty.Data;
using backend_loyalty.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace backend_loyalty.Repositories;

/// <summary>
/// Unit of Work pattern implementation
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    public ILoyaltyAccountRepository LoyaltyAccounts { get; }
    public ILoyaltyTransactionRepository LoyaltyTransactions { get; }
    public ILoyaltyRedemptionRepository LoyaltyRedemptions { get; }
    public ApplicationDbContext Context => _context;

    public UnitOfWork(
        ApplicationDbContext context,
        ILoyaltyAccountRepository loyaltyAccountRepository,
        ILoyaltyTransactionRepository loyaltyTransactionRepository,
        ILoyaltyRedemptionRepository loyaltyRedemptionRepository)
    {
        _context = context;
        LoyaltyAccounts = loyaltyAccountRepository;
        LoyaltyTransactions = loyaltyTransactionRepository;
        LoyaltyRedemptions = loyaltyRedemptionRepository;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public IExecutionStrategy CreateExecutionStrategy()
    {
        return _context.Database.CreateExecutionStrategy();
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
