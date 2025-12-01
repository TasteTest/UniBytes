using backend_monolith.Modelss;

namespace backend_monolith.Repositories.Interfaces;

/// <summary>
/// User analytics-specific repository interface
/// </summary>
public interface IUserAnalyticsRepository : IRepository<UserAnalytics>
{
    Task<IEnumerable<UserAnalytics>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserAnalytics>> GetBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserAnalytics>> GetByEventTypeAsync(string eventType, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserAnalytics>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}

