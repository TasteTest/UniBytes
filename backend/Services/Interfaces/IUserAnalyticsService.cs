using backend.Common;
using backend.DTOs.UserAnalytics.Request;
using backend.DTOs.UserAnalytics.Response;

namespace backend.Services.Interfaces;

/// <summary>
/// User analytics service interface
/// </summary>
public interface IUserAnalyticsService
{
    Task<Result<UserAnalyticsResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<UserAnalyticsResponse>>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<UserAnalyticsResponse>>> GetBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<UserAnalyticsResponse>>> GetByEventTypeAsync(string eventType, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<UserAnalyticsResponse>>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<Result<UserAnalyticsResponse>> CreateAsync(CreateUserAnalyticsRequest createRequest, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
