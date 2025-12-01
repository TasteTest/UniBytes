using backend_monolith.Data;
using backend_monolith.Modelss;
using backend_monolith.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend_monolith.Repositories;

/// <summary>
/// LoyaltyAccount repository implementation
/// </summary>
public class LoyaltyAccountRepository : Repository<LoyaltyAccount>, ILoyaltyAccountRepository
{
    public LoyaltyAccountRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<LoyaltyAccount?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(la => la.UserId == userId, cancellationToken);
    }

    public async Task<LoyaltyAccount?> GetByUserIdWithTransactionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(la => la.LoyaltyTransactions.OrderByDescending(t => t.CreatedAt))
            .FirstOrDefaultAsync(la => la.UserId == userId, cancellationToken);
    }

    public async Task<LoyaltyAccount?> GetByUserIdWithRedemptionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(la => la.LoyaltyRedemptions.OrderByDescending(r => r.CreatedAt))
            .FirstOrDefaultAsync(la => la.UserId == userId, cancellationToken);
    }

    public async Task<LoyaltyAccount?> GetByUserIdWithAllDataAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(la => la.LoyaltyTransactions.OrderByDescending(t => t.CreatedAt))
            .Include(la => la.LoyaltyRedemptions.OrderByDescending(r => r.CreatedAt))
            .FirstOrDefaultAsync(la => la.UserId == userId, cancellationToken);
    }

    public async Task<IEnumerable<LoyaltyAccount>> GetActiveAccountsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(la => la.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<LoyaltyAccount>> GetByTierAsync(int tier, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(la => (int)la.Tier == tier && la.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> UserHasAccountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(la => la.UserId == userId, cancellationToken);
    }

    public async Task<bool> AddPointsAsync(Guid userId, long points, string reason, Guid? referenceId = null, CancellationToken cancellationToken = default)
    {
        var account = await GetByUserIdAsync(userId, cancellationToken);
        if (account == null) return false;

        account.PointsBalance += points;
        account.UpdatedAt = DateTime.UtcNow;

        var transaction = new LoyaltyTransaction
        {
            LoyaltyAccountId = account.Id,
            ChangeAmount = points,
            Reason = reason,
            ReferenceId = referenceId,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Set<LoyaltyTransaction>().AddAsync(transaction, cancellationToken);
        return true;
    }

    public async Task<bool> DeductPointsAsync(Guid userId, long points, string reason, Guid? referenceId = null, CancellationToken cancellationToken = default)
    {
        var account = await GetByUserIdAsync(userId, cancellationToken);
        if (account == null || account.PointsBalance < points) return false;

        account.PointsBalance -= points;
        account.UpdatedAt = DateTime.UtcNow;

        var transaction = new LoyaltyTransaction
        {
            LoyaltyAccountId = account.Id,
            ChangeAmount = -points,
            Reason = reason,
            ReferenceId = referenceId,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Set<LoyaltyTransaction>().AddAsync(transaction, cancellationToken);
        return true;
    }
}
