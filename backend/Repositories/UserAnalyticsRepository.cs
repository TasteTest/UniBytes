using backend.Data;
using backend.Models;
using backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories;

/// <summary>
/// User analytics repository implementation
/// </summary>
public class UserAnalyticsRepository(ApplicationDbContext context)
    : Repository<UserAnalytics>(context), IUserAnalyticsRepository
{
    public async Task<IEnumerable<UserAnalytics>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UserAnalytics>> GetBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(a => a.SessionId == sessionId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UserAnalytics>> GetByEventTypeAsync(string eventType, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(a => a.EventType == eventType)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UserAnalytics>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(a => a.CreatedAt >= startDate && a.CreatedAt <= endDate)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}

