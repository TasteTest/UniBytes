using System.Text.Json;

namespace backend.DTOs.Order.Request;

/// <summary>
/// DTO for creating an order
/// </summary>
public record CreateOrderRequest(
    Guid UserId,
    List<CreateOrderItemRequest> OrderItems,
    string Currency = "ron",
    JsonElement? Metadata = null
);
