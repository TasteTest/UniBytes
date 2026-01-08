using backend.DTOs.AI.Request;
using backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

/// <summary>
/// API controller for AI menu recommendation functionality.
/// </summary>
[ApiController]
[Route("api/ai")]
public class AIController : ControllerBase
{
    private readonly IAIService _aiService;
    private readonly ILogger<AIController> _logger;

    public AIController(IAIService aiService, ILogger<AIController> logger)
    {
        _aiService = aiService;
        _logger = logger;
    }

    /// <summary>
    /// Get personalized menu recommendations based on user preferences.
    /// </summary>
    /// <param name="request">User preferences for menu generation including dietary goals, restrictions, and preferences.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>AI-generated menu recommendations with reasoning.</returns>
    [HttpPost("chat")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Chat([FromBody] AIRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _aiService.GetMenuRecommendationsAsync(request, cancellationToken);

        if (result.IsSuccess && result.Data != null)
        {
            return Ok(result.Data);
        }

        _logger.LogWarning("AI menu recommendation failed: {Error}", result.Error);
        return StatusCode(500, new { error = result.Error });
    }
}
