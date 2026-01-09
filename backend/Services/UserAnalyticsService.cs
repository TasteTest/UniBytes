using AutoMapper;
using backend.Common;
using backend.Models;
using backend.Repositories.Interfaces;
using backend.Services.Interfaces;
using backend.DTOs.UserAnalytics.Request;
using backend.DTOs.UserAnalytics.Response;

namespace backend.Services;

/// <summary>
/// User analytics service implementation
/// </summary>
public class UserAnalyticsService(
    IUserAnalyticsRepository userAnalyticsRepository,
    IMapper mapper,
    ILogger<UserAnalyticsService> logger)
    : IUserAnalyticsService
{
    public async Task<Result<UserAnalyticsResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var analytics = await userAnalyticsRepository.GetByIdAsync(id, cancellationToken);
            if (analytics == null)
            {
                return Result<UserAnalyticsResponse>.Failure($"User analytics with ID {id} not found");
            }

            var analyticsResponse = mapper.Map<UserAnalyticsResponse>(analytics);
            return Result<UserAnalyticsResponse>.Success(analyticsResponse);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting user analytics by ID {AnalyticsId}", id);
            return Result<UserAnalyticsResponse>.Failure($"Error retrieving user analytics: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<UserAnalyticsResponse>>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var analytics = await userAnalyticsRepository.GetByUserIdAsync(userId, cancellationToken);
            var analyticsResponses = mapper.Map<IEnumerable<UserAnalyticsResponse>>(analytics);
            return Result<IEnumerable<UserAnalyticsResponse>>.Success(analyticsResponses);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting user analytics for user {UserId}", userId);
            return Result<IEnumerable<UserAnalyticsResponse>>.Failure($"Error retrieving user analytics: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<UserAnalyticsResponse>>> GetBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var analytics = await userAnalyticsRepository.GetBySessionIdAsync(sessionId, cancellationToken);
            var analyticsResponses = mapper.Map<IEnumerable<UserAnalyticsResponse>>(analytics);
            return Result<IEnumerable<UserAnalyticsResponse>>.Success(analyticsResponses);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting user analytics for session {SessionId}", sessionId);
            return Result<IEnumerable<UserAnalyticsResponse>>.Failure($"Error retrieving user analytics: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<UserAnalyticsResponse>>> GetByEventTypeAsync(string eventType, CancellationToken cancellationToken = default)
    {
        try
        {
            var analytics = await userAnalyticsRepository.GetByEventTypeAsync(eventType, cancellationToken);
            var analyticsResponses = mapper.Map<IEnumerable<UserAnalyticsResponse>>(analytics);
            return Result<IEnumerable<UserAnalyticsResponse>>.Success(analyticsResponses);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting user analytics for event type {EventType}", eventType);
            return Result<IEnumerable<UserAnalyticsResponse>>.Failure($"Error retrieving user analytics: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<UserAnalyticsResponse>>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        try
        {
            var analytics = await userAnalyticsRepository.GetByDateRangeAsync(startDate, endDate, cancellationToken);
            var analyticsResponses = mapper.Map<IEnumerable<UserAnalyticsResponse>>(analytics);
            return Result<IEnumerable<UserAnalyticsResponse>>.Success(analyticsResponses);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting user analytics for date range {StartDate} - {EndDate}", startDate, endDate);
            return Result<IEnumerable<UserAnalyticsResponse>>.Failure($"Error retrieving user analytics: {ex.Message}");
        }
    }

    public async Task<Result<UserAnalyticsResponse>> CreateAsync(CreateUserAnalyticsRequest createRequest, CancellationToken cancellationToken = default)
    {
        try
        {
            // Note: User validation is handled at controller/auth level
            var analytics = mapper.Map<UserAnalytics>(createRequest);
            analytics.CreatedAt = DateTime.UtcNow;

            await userAnalyticsRepository.AddAsync(analytics, cancellationToken);

            var analyticsResponse = mapper.Map<UserAnalyticsResponse>(analytics);
            logger.LogInformation("Created user analytics event {EventType} for user {UserId}", createRequest.EventType, createRequest.UserId);
            return Result<UserAnalyticsResponse>.Success(analyticsResponse);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating user analytics");
            return Result<UserAnalyticsResponse>.Failure($"Error creating user analytics: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var analytics = await userAnalyticsRepository.GetByIdAsync(id, cancellationToken);
            if (analytics == null)
            {
                return Result.Failure($"User analytics with ID {id} not found");
            }

            await userAnalyticsRepository.DeleteAsync(analytics, cancellationToken);

            logger.LogInformation("Deleted user analytics with ID {AnalyticsId}", id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting user analytics {AnalyticsId}", id);
            return Result.Failure($"Error deleting user analytics: {ex.Message}");
        }
    }
}
