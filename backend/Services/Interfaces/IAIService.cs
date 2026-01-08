using backend.Common;
using backend.DTOs.AI.Request;
using backend.DTOs.AI.Response;

namespace backend.Services.Interfaces;

/// <summary>
/// Service interface for AI menu recommendation functionality.
/// </summary>
public interface IAIService
{
    /// <summary>
    /// Get personalized menu recommendations based on user preferences.
    /// </summary>
    /// <param name="request">User preferences for menu generation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>AI response with menu recommendations and optional reasoning.</returns>
    Task<Result<AIResponse>> GetMenuRecommendationsAsync(AIRequest request, CancellationToken cancellationToken = default);
}
