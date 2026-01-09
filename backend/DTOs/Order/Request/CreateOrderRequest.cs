using System.Text.Json;
using System.Text.Json.Serialization;

namespace backend.DTOs.Order.Request;

/// <summary>
/// DTO for creating an order
/// </summary>
public record CreateOrderRequest(
    [property: JsonRequired] Guid UserId,
    List<CreateOrderItemRequest> OrderItems,
    string Currency = "ron",
    JsonElement? Metadata = null
);
