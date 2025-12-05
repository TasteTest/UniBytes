using System.Text.Json;

namespace backend.DTOs.Order.Request;

/// <summary>
/// DTO for creating an order item
/// </summary>
public record CreateOrderItemRequest(
    Guid MenuItemId,
    string Name,
    decimal UnitPrice,
    int Quantity,
    JsonElement? Modifiers = null
);

