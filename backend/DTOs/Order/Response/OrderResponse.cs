using System.Text.Json;

namespace backend.DTOs.Order.Response;

/// <summary>
/// DTO for order response
/// </summary>
public class OrderResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? ExternalUserRef { get; set; }
    public decimal TotalAmount { get; set; }
    public required string Currency { get; set; }
    public required string PaymentStatus { get; set; }
    public required string OrderStatus { get; set; }
    public DateTime CreatedAt { get; set; }
    public JsonElement? Metadata { get; set; }
    public List<OrderItemResponse> OrderItems { get; set; } = new();
}

/// <summary>
/// DTO for order item response
/// </summary>
public class OrderItemResponse
{
    public Guid Id { get; set; }
    public Guid? MenuItemId { get; set; }
    public required string Name { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public JsonElement? Modifiers { get; set; }
}

