using backend.Data;
using backend.Modelss;
using backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories;

/// <summary>
/// LoyaltyTransaction repository implementation
/// </summary>
public class LoyaltyTransactionRepository : Repository<LoyaltyTransaction>, ILoyaltyTransactionRepository
{
    public LoyaltyTransactionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<LoyaltyTransaction>> GetByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(lt => lt.LoyaltyAccountId == accountId)
            .OrderByDescending(lt => lt.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<LoyaltyTransaction>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(lt => lt.LoyaltyAccount)
            .Where(lt => lt.LoyaltyAccount.UserId == userId)
            .OrderByDescending(lt => lt.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<LoyaltyTransaction>> GetByReferenceIdAsync(Guid referenceId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(lt => lt.ReferenceId == referenceId)
            .OrderByDescending(lt => lt.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<LoyaltyTransaction>> GetRecentTransactionsAsync(Guid accountId, int count, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(lt => lt.LoyaltyAccountId == accountId)
            .OrderByDescending(lt => lt.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }
}
