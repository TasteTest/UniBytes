using System.Text.Json;
using System.Text.Json.Serialization;

namespace backend.DTOs.Payment.Request;

/// <summary>
/// Request to create a Stripe checkout session
/// </summary>
public class CreateCheckoutSessionRequest
{
    [JsonRequired]
    public Guid OrderId { get; init; }
    public string AccessToken { get; set; } = string.Empty;
    public string UserEmail { get; init; } = string.Empty;
    public List<CheckoutLineItem> LineItems { get; init; } = new();
    public string SuccessUrl { get; init; } = string.Empty;
    public string CancelUrl { get; init; } = string.Empty;
    public string? IdempotencyKey { get; init; }
    public JsonElement? Metadata { get; init; }
}

/// <summary>
/// Line item for checkout
/// </summary>
public class CheckoutLineItem
{
    public Guid? MenuItemId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal UnitPrice { get; init; }
    public int Quantity { get; init; }
    public string Currency { get; init; } = "ron";
    public string? ImageUrl { get; init; }
    public JsonElement? Modifiers { get; init; }
    public bool IsReward { get; init; } = false;
    public string? RewardId { get; init; }
}

