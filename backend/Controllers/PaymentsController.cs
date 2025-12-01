using backend.DTOs.Request;
using backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

/// <summary>
/// API controller for payment operations
/// </summary>
[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly IStripeService _stripeService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        IPaymentService paymentService,
        IStripeService stripeService,
        ILogger<PaymentsController> logger)
    {
        _paymentService = paymentService;
        _stripeService = stripeService;
        _logger = logger;
    }

    /// <summary>
    /// Get payment by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPaymentById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _paymentService.GetPaymentByIdAsync(id, cancellationToken);
        
        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }
        
        return NotFound(new { error = result.Error });
    }

    /// <summary>
    /// Get payment by order ID
    /// </summary>
    [HttpGet("order/{orderId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPaymentByOrderId(Guid orderId, CancellationToken cancellationToken)
    {
        var result = await _paymentService.GetPaymentByOrderIdAsync(orderId, cancellationToken);
        
        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }
        
        return NotFound(new { error = result.Error });
    }

    /// <summary>
    /// Get all payments for a user
    /// </summary>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaymentsByUserId(Guid userId, CancellationToken cancellationToken)
    {
        var result = await _paymentService.GetPaymentsByUserIdAsync(userId, cancellationToken);
        
        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }
        
        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Create a Stripe checkout session
    /// </summary>
    [HttpPost("checkout-session")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCheckoutSession(
        [FromBody] CreateCheckoutSessionRequest request, 
        CancellationToken cancellationToken)
    {
        var result = await _stripeService.CreateCheckoutSessionAsync(request, cancellationToken);
        
        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }
        
        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Verify payment status
    /// </summary>
    [HttpGet("verify/{sessionId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyPayment(string sessionId, CancellationToken cancellationToken)
    {
        var result = await _stripeService.VerifyPaymentAsync(sessionId, cancellationToken);
        
        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }
        
        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Stripe webhook endpoint
    /// </summary>
    [HttpPost("webhook")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> StripeWebhook(CancellationToken cancellationToken)
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync(cancellationToken);
        var stripeSignature = Request.Headers["Stripe-Signature"].ToString();

        if (string.IsNullOrEmpty(stripeSignature))
        {
            return BadRequest(new { error = "Missing Stripe signature" });
        }

        var result = await _stripeService.HandleWebhookEventAsync(json, stripeSignature, cancellationToken);
        
        if (result.IsSuccess)
        {
            return Ok();
        }
        
        return BadRequest(new { error = result.Error });
    }
}

