using backend.Data;
using backend.Models;
using backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories;

/// <summary>
/// LoyaltyAccount repository implementation
/// </summary>
public class LoyaltyAccountRepository(ApplicationDbContext context)
    : Repository<LoyaltyAccount>(context), ILoyaltyAccountRepository
{
    public async Task<LoyaltyAccount?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(la => la.UserId == userId, cancellationToken);
    }

    public async Task<LoyaltyAccount?> GetByUserIdWithTransactionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(la => la.LoyaltyTransactions.OrderByDescending(t => t.CreatedAt))
            .FirstOrDefaultAsync(la => la.UserId == userId, cancellationToken);
    }

    public async Task<LoyaltyAccount?> GetByUserIdWithRedemptionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(la => la.LoyaltyRedemptions.OrderByDescending(r => r.CreatedAt))
            .FirstOrDefaultAsync(la => la.UserId == userId, cancellationToken);
    }

    public async Task<LoyaltyAccount?> GetByUserIdWithAllDataAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(la => la.LoyaltyTransactions.OrderByDescending(t => t.CreatedAt))
            .Include(la => la.LoyaltyRedemptions.OrderByDescending(r => r.CreatedAt))
            .FirstOrDefaultAsync(la => la.UserId == userId, cancellationToken);
    }

    public async Task<IEnumerable<LoyaltyAccount>> GetActiveAccountsAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(la => la.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<LoyaltyAccount>> GetByTierAsync(int tier, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(la => (int)la.Tier == tier && la.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> UserHasAccountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
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

        await Context.Set<LoyaltyTransaction>().AddAsync(transaction, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
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

        await Context.Set<LoyaltyTransaction>().AddAsync(transaction, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
