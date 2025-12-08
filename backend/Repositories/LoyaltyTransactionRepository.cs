using backend.Data;
using backend.Models;
using backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories;

/// <summary>
/// LoyaltyTransaction repository implementation
/// </summary>
public class LoyaltyTransactionRepository(ApplicationDbContext context)
    : Repository<LoyaltyTransaction>(context), ILoyaltyTransactionRepository
{
    public async Task<IEnumerable<LoyaltyTransaction>> GetByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(lt => lt.LoyaltyAccountId == accountId)
            .OrderByDescending(lt => lt.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<LoyaltyTransaction>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(lt => lt.LoyaltyAccount)
            .Where(lt => lt.LoyaltyAccount.UserId == userId)
            .OrderByDescending(lt => lt.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<LoyaltyTransaction>> GetByReferenceIdAsync(Guid referenceId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(lt => lt.ReferenceId == referenceId)
            .OrderByDescending(lt => lt.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<LoyaltyTransaction>> GetRecentTransactionsAsync(Guid accountId, int count, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(lt => lt.LoyaltyAccountId == accountId)
            .OrderByDescending(lt => lt.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }
}
