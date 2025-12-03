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
    public JsonDocument? Metadata { get; set; }
}

