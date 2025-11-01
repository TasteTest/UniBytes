using backend_user.Common;
using backend_user.DTOs.Request;
using backend_user.DTOs.Response;

namespace backend_user.Services.Interfaces;

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
