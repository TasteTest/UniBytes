using backend.Modelss;

namespace backend.Repositories.Interfaces;

/// <summary>
/// LoyaltyTransaction-specific repository interface
/// </summary>
public interface ILoyaltyTransactionRepository : IRepository<LoyaltyTransaction>
{
    Task<IEnumerable<LoyaltyTransaction>> GetByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<IEnumerable<LoyaltyTransaction>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<LoyaltyTransaction>> GetByReferenceIdAsync(Guid referenceId, CancellationToken cancellationToken = default);
    Task<IEnumerable<LoyaltyTransaction>> GetRecentTransactionsAsync(Guid accountId, int count, CancellationToken cancellationToken = default);
}
