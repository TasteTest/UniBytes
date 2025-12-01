using backend_monolith.Modelss;

namespace backend_monolith.Repositories.Interfaces;

/// <summary>
/// LoyaltyRedemption-specific repository interface
/// </summary>
public interface ILoyaltyRedemptionRepository : IRepository<LoyaltyRedemption>
{
    Task<IEnumerable<LoyaltyRedemption>> GetByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<IEnumerable<LoyaltyRedemption>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<LoyaltyRedemption>> GetByRewardTypeAsync(string rewardType, CancellationToken cancellationToken = default);
    Task<IEnumerable<LoyaltyRedemption>> GetRecentRedemptionsAsync(Guid accountId, int count, CancellationToken cancellationToken = default);
    Task<long> GetTotalPointsRedeemedAsync(Guid accountId, CancellationToken cancellationToken = default);
}
