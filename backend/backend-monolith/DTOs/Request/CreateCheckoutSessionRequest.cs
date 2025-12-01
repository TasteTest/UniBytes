namespace backend_monolith.DTOs.Request;

/// <summary>
/// Request to create a Stripe checkout session
/// </summary>
public class CreateCheckoutSessionRequest
{
    public Guid OrderId { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public List<CheckoutLineItem> LineItems { get; set; } = new();
    public string SuccessUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
    public string? IdempotencyKey { get; set; }
}

/// <summary>
/// Line item for checkout
/// </summary>
public class CheckoutLineItem
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public string? ImageUrl { get; set; }
}

