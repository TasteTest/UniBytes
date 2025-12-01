using backend.DTOs.Request;
using backend.DTOs.Response;
using backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

/// <summary>
/// Loyalty accounts controller for managing loyalty points
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class LoyaltyAccountsController : ControllerBase
{
    private readonly ILoyaltyAccountService _loyaltyAccountService;
    private readonly ILogger<LoyaltyAccountsController> _logger;

    public LoyaltyAccountsController(ILoyaltyAccountService loyaltyAccountService, ILogger<LoyaltyAccountsController> logger)
    {
        _loyaltyAccountService = loyaltyAccountService;
        _logger = logger;
    }

    /// <summary>
    /// Get all loyalty accounts
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<LoyaltyAccountResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _loyaltyAccountService.GetAllAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
    }

    /// <summary>
    /// Get active loyalty accounts only
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(IEnumerable<LoyaltyAccountResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActive(CancellationToken cancellationToken)
    {
        var result = await _loyaltyAccountService.GetActiveAccountsAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
    }

    /// <summary>
    /// Get loyalty accounts by tier
    /// </summary>
    [HttpGet("tier/{tier:int}")]
    [ProducesResponseType(typeof(IEnumerable<LoyaltyAccountResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByTier(int tier, CancellationToken cancellationToken)
    {
        var result = await _loyaltyAccountService.GetByTierAsync(tier, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
    }

    /// <summary>
    /// Get loyalty account by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(LoyaltyAccountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _loyaltyAccountService.GetByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : NotFound(result.Error);
    }

    /// <summary>
    /// Get loyalty account by user ID
    /// </summary>
    [HttpGet("user/{userId:guid}")]
    [ProducesResponseType(typeof(LoyaltyAccountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByUserId(Guid userId, CancellationToken cancellationToken)
    {
        var result = await _loyaltyAccountService.GetByUserIdAsync(userId, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : NotFound(result.Error);
    }

    /// <summary>
    /// Get complete loyalty account details with history
    /// </summary>
    [HttpGet("user/{userId:guid}/details")]
    [ProducesResponseType(typeof(LoyaltyAccountDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAccountDetails(Guid userId, CancellationToken cancellationToken)
    {
        var result = await _loyaltyAccountService.GetAccountDetailsAsync(userId, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : NotFound(result.Error);
    }

    /// <summary>
    /// Get user's points balance
    /// </summary>
    [HttpGet("user/{userId:guid}/balance")]
    [ProducesResponseType(typeof(long), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPointsBalance(Guid userId, CancellationToken cancellationToken)
    {
        var result = await _loyaltyAccountService.GetPointsBalanceAsync(userId, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : NotFound(result.Error);
    }

    /// <summary>
    /// Create a new loyalty account
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(LoyaltyAccountResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateLoyaltyAccountRequest createRequest, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _loyaltyAccountService.CreateAsync(createRequest, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(result.Error);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    /// <summary>
    /// Update an existing loyalty account
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(LoyaltyAccountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLoyaltyAccountRequest updateRequest, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _loyaltyAccountService.UpdateAsync(id, updateRequest, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : NotFound(result.Error);
    }

    /// <summary>
    /// Delete a loyalty account
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _loyaltyAccountService.DeleteAsync(id, cancellationToken);
        return result.IsSuccess ? NoContent() : NotFound(result.Error);
    }

    /// <summary>
    /// Add points to a user's account
    /// </summary>
    [HttpPost("add-points")]
    [ProducesResponseType(typeof(LoyaltyAccountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddPoints([FromBody] AddPointsRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _loyaltyAccountService.AddPointsAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
    }

    /// <summary>
    /// Redeem points for a reward
    /// </summary>
    [HttpPost("redeem-points")]
    [ProducesResponseType(typeof(LoyaltyRedemptionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RedeemPoints([FromBody] RedeemPointsRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _loyaltyAccountService.RedeemPointsAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
    }
}
