using AutoMapper;
using backend.Common;
using backend.DTOs.Request;
using backend.DTOs.Response;
using backend.Modelss;
using backend.Repositories.Interfaces;
using backend.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace backend.Services;

/// <summary>
/// User analytics service implementation
/// </summary>
public class UserAnalyticsService : IUserAnalyticsService
{
    private readonly IUserAnalyticsRepository _userAnalyticsRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<UserAnalyticsService> _logger;

    public UserAnalyticsService(IUserAnalyticsRepository userAnalyticsRepository, IMapper mapper, ILogger<UserAnalyticsService> logger)
    {
        _userAnalyticsRepository = userAnalyticsRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<UserAnalyticsResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var analytics = await _userAnalyticsRepository.GetByIdAsync(id, cancellationToken);
            if (analytics == null)
            {
                return Result<UserAnalyticsResponse>.Failure($"User analytics with ID {id} not found");
            }

            var analyticsResponse = _mapper.Map<UserAnalyticsResponse>(analytics);
            return Result<UserAnalyticsResponse>.Success(analyticsResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user analytics by ID {AnalyticsId}", id);
            return Result<UserAnalyticsResponse>.Failure($"Error retrieving user analytics: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<UserAnalyticsResponse>>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var analytics = await _userAnalyticsRepository.GetByUserIdAsync(userId, cancellationToken);
            var analyticsResponses = _mapper.Map<IEnumerable<UserAnalyticsResponse>>(analytics);
            return Result<IEnumerable<UserAnalyticsResponse>>.Success(analyticsResponses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user analytics for user {UserId}", userId);
            return Result<IEnumerable<UserAnalyticsResponse>>.Failure($"Error retrieving user analytics: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<UserAnalyticsResponse>>> GetBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var analytics = await _userAnalyticsRepository.GetBySessionIdAsync(sessionId, cancellationToken);
            var analyticsResponses = _mapper.Map<IEnumerable<UserAnalyticsResponse>>(analytics);
            return Result<IEnumerable<UserAnalyticsResponse>>.Success(analyticsResponses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user analytics for session {SessionId}", sessionId);
            return Result<IEnumerable<UserAnalyticsResponse>>.Failure($"Error retrieving user analytics: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<UserAnalyticsResponse>>> GetByEventTypeAsync(string eventType, CancellationToken cancellationToken = default)
    {
        try
        {
            var analytics = await _userAnalyticsRepository.GetByEventTypeAsync(eventType, cancellationToken);
            var analyticsResponses = _mapper.Map<IEnumerable<UserAnalyticsResponse>>(analytics);
            return Result<IEnumerable<UserAnalyticsResponse>>.Success(analyticsResponses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user analytics for event type {EventType}", eventType);
            return Result<IEnumerable<UserAnalyticsResponse>>.Failure($"Error retrieving user analytics: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<UserAnalyticsResponse>>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        try
        {
            var analytics = await _userAnalyticsRepository.GetByDateRangeAsync(startDate, endDate, cancellationToken);
            var analyticsResponses = _mapper.Map<IEnumerable<UserAnalyticsResponse>>(analytics);
            return Result<IEnumerable<UserAnalyticsResponse>>.Success(analyticsResponses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user analytics for date range {StartDate} - {EndDate}", startDate, endDate);
            return Result<IEnumerable<UserAnalyticsResponse>>.Failure($"Error retrieving user analytics: {ex.Message}");
        }
    }

    public async Task<Result<UserAnalyticsResponse>> CreateAsync(CreateUserAnalyticsRequest createRequest, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if user exists
            // Note: In monolith, we assume user exists. In production, inject IUserRepository if needed
            var userExists = true; // User validation removed - handled at controller/auth level
            if (!userExists)
            {
                return Result<UserAnalyticsResponse>.Failure($"User with ID {createRequest.UserId} not found");
            }

            var analytics = _mapper.Map<UserAnalytics>(createRequest);
            analytics.CreatedAt = DateTime.UtcNow;

            await _userAnalyticsRepository.AddAsync(analytics, cancellationToken);

            var analyticsResponse = _mapper.Map<UserAnalyticsResponse>(analytics);
            _logger.LogInformation("Created user analytics event {EventType} for user {UserId}", createRequest.EventType, createRequest.UserId);
            return Result<UserAnalyticsResponse>.Success(analyticsResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user analytics");
            return Result<UserAnalyticsResponse>.Failure($"Error creating user analytics: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var analytics = await _userAnalyticsRepository.GetByIdAsync(id, cancellationToken);
            if (analytics == null)
            {
                return Result.Failure($"User analytics with ID {id} not found");
            }

            await _userAnalyticsRepository.DeleteAsync(analytics, cancellationToken);

            _logger.LogInformation("Deleted user analytics with ID {AnalyticsId}", id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user analytics {AnalyticsId}", id);
            return Result.Failure($"Error deleting user analytics: {ex.Message}");
        }
    }
}
