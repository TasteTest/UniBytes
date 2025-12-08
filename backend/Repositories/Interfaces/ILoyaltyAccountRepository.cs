using backend.Models;

namespace backend.Repositories.Interfaces;

/// <summary>
/// LoyaltyAccount-specific repository interface
/// </summary>
public interface ILoyaltyAccountRepository : IRepository<LoyaltyAccount>
{
    Task<LoyaltyAccount?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<LoyaltyAccount?> GetByUserIdWithTransactionsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<LoyaltyAccount?> GetByUserIdWithRedemptionsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<LoyaltyAccount?> GetByUserIdWithAllDataAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<LoyaltyAccount>> GetActiveAccountsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<LoyaltyAccount>> GetByTierAsync(int tier, CancellationToken cancellationToken = default);
    Task<bool> UserHasAccountAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> AddPointsAsync(Guid userId, long points, string reason, Guid? referenceId = null, CancellationToken cancellationToken = default);
    Task<bool> DeductPointsAsync(Guid userId, long points, string reason, Guid? referenceId = null, CancellationToken cancellationToken = default);
}
