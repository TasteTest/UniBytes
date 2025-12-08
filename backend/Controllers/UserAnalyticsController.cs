using backend.Services.Interfaces;
using backend.DTOs.UserAnalytics.Request;
using backend.DTOs.UserAnalytics.Response;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

/// <summary>
/// User analytics controller for CRUD operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UserAnalyticsController(
    IUserAnalyticsService userAnalyticsService,
    ILogger<UserAnalyticsController> logger)
    : ControllerBase
{
    private readonly ILogger<UserAnalyticsController> _logger = logger;

    /// <summary>
    /// Get user analytics by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserAnalyticsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await userAnalyticsService.GetByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : NotFound(result.Error);
    }

    /// <summary>
    /// Get all analytics for a user
    /// </summary>
    [HttpGet("user/{userId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<UserAnalyticsResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByUserId(Guid userId, CancellationToken cancellationToken)
    {
        var result = await userAnalyticsService.GetByUserIdAsync(userId, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
    }

    /// <summary>
    /// Get all analytics for a session
    /// </summary>
    [HttpGet("session/{sessionId}")]
    [ProducesResponseType(typeof(IEnumerable<UserAnalyticsResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBySessionId(string sessionId, CancellationToken cancellationToken)
    {
        var result = await userAnalyticsService.GetBySessionIdAsync(sessionId, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
    }

    /// <summary>
    /// Get all analytics by event type
    /// </summary>
    [HttpGet("event/{eventType}")]
    [ProducesResponseType(typeof(IEnumerable<UserAnalyticsResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByEventType(string eventType, CancellationToken cancellationToken)
    {
        var result = await userAnalyticsService.GetByEventTypeAsync(eventType, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
    }

    /// <summary>
    /// Get analytics by date range
    /// </summary>
    [HttpGet("date-range")]
    [ProducesResponseType(typeof(IEnumerable<UserAnalyticsResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, CancellationToken cancellationToken)
    {
        var result = await userAnalyticsService.GetByDateRangeAsync(startDate, endDate, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
    }

    /// <summary>
    /// Create a new analytics event
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(UserAnalyticsResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateUserAnalyticsRequest createRequest, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await userAnalyticsService.CreateAsync(createRequest, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(result.Error);
        }

        return CreatedAtAction(nameof(GetById), new { id = Guid.NewGuid() }, result.Data);
    }

    /// <summary>
    /// Delete an analytics event
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await userAnalyticsService.DeleteAsync(id, cancellationToken);
        return result.IsSuccess ? NoContent() : NotFound(result.Error);
    }
}
