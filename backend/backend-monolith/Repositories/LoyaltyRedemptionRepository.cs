using backend_monolith.Data;
using backend_monolith.Modelss;
using backend_monolith.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend_monolith.Repositories;

/// <summary>
/// LoyaltyRedemption repository implementation
/// </summary>
public class LoyaltyRedemptionRepository : Repository<LoyaltyRedemption>, ILoyaltyRedemptionRepository
{
    public LoyaltyRedemptionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<LoyaltyRedemption>> GetByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(lr => lr.LoyaltyAccountId == accountId)
            .OrderByDescending(lr => lr.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<LoyaltyRedemption>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(lr => lr.LoyaltyAccount)
            .Where(lr => lr.LoyaltyAccount.UserId == userId)
            .OrderByDescending(lr => lr.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<LoyaltyRedemption>> GetByRewardTypeAsync(string rewardType, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(lr => lr.RewardType == rewardType)
            .OrderByDescending(lr => lr.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<LoyaltyRedemption>> GetRecentRedemptionsAsync(Guid accountId, int count, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(lr => lr.LoyaltyAccountId == accountId)
            .OrderByDescending(lr => lr.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<long> GetTotalPointsRedeemedAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(lr => lr.LoyaltyAccountId == accountId)
            .SumAsync(lr => lr.PointsUsed, cancellationToken);
    }
}
