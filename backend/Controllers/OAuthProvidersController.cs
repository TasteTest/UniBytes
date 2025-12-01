using backend.Common.Enums;
using backend.DTOs.Request;
using backend.DTOs.Response;
using backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

/// <summary>
/// OAuth providers controller for CRUD operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class OAuthProvidersController : ControllerBase
{
    private readonly IOAuthProviderService _oAuthProviderService;
    private readonly ILogger<OAuthProvidersController> _logger;

    public OAuthProvidersController(IOAuthProviderService oAuthProviderService, ILogger<OAuthProvidersController> logger)
    {
        _oAuthProviderService = oAuthProviderService;
        _logger = logger;
    }

    /// <summary>
    /// Get OAuth provider by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OAuthProviderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _oAuthProviderService.GetByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : NotFound(result.Error);
    }

    /// <summary>
    /// Get all OAuth providers for a user
    /// </summary>
    [HttpGet("user/{userId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<OAuthProviderResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByUserId(Guid userId, CancellationToken cancellationToken)
    {
        var result = await _oAuthProviderService.GetByUserIdAsync(userId, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
    }

    /// <summary>
    /// Get OAuth provider by provider type and provider ID
    /// </summary>
    [HttpGet("provider/{provider}/{providerId}")]
    [ProducesResponseType(typeof(OAuthProviderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByProviderAndProviderId(OAuthProviderType provider, string providerId, CancellationToken cancellationToken)
    {
        var result = await _oAuthProviderService.GetByProviderAndProviderIdAsync(provider, providerId, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : NotFound(result.Error);
    }

    /// <summary>
    /// Create a new OAuth provider
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(OAuthProviderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateOAuthProviderRequest createRequest, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _oAuthProviderService.CreateAsync(createRequest, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(result.Error);
        }

        return CreatedAtAction(nameof(GetById), new { id = Guid.NewGuid() }, result.Data);
    }

    /// <summary>
    /// Update an existing OAuth provider
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(OAuthProviderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOAuthProviderRequest updateRequest, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _oAuthProviderService.UpdateAsync(id, updateRequest, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : NotFound(result.Error);
    }

    /// <summary>
    /// Delete an OAuth provider
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _oAuthProviderService.DeleteAsync(id, cancellationToken);
        return result.IsSuccess ? NoContent() : NotFound(result.Error);
    }
}
