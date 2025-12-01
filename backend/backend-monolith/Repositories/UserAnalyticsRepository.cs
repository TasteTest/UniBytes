using backend_monolith.Data;
using backend_monolith.Modelss;
using backend_monolith.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend_monolith.Repositories;

/// <summary>
/// User analytics repository implementation
/// </summary>
public class UserAnalyticsRepository : Repository<UserAnalytics>, IUserAnalyticsRepository
{
    public UserAnalyticsRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<UserAnalytics>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UserAnalytics>> GetBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.SessionId == sessionId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UserAnalytics>> GetByEventTypeAsync(string eventType, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.EventType == eventType)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UserAnalytics>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.CreatedAt >= startDate && a.CreatedAt <= endDate)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}

