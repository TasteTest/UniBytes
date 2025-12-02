namespace backend.DTOs.Payment.Request;

/// <summary>
/// Request to create a Stripe checkout session
/// </summary>
public class CreateCheckoutSessionRequest
{
    public Guid OrderId { get; init; }
    public string AccessToken { get; set; } = string.Empty;
    public string UserEmail { get; init; } = string.Empty;
    public List<CheckoutLineItem> LineItems { get; init; } = new();
    public string SuccessUrl { get; init; } = string.Empty;
    public string CancelUrl { get; init; } = string.Empty;
    public string? IdempotencyKey { get; init; }
}

/// <summary>
/// Line item for checkout
/// </summary>
public class CheckoutLineItem
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal UnitPrice { get; init; }
    public int Quantity { get; init; }
    public string? ImageUrl { get; init; }
}

