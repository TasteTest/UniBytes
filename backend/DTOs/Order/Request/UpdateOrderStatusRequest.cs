using System.Text.Json.Serialization;

namespace backend.DTOs.Order.Request;

/// <summary>
/// DTO for updating order status
/// </summary>
public record UpdateOrderStatusRequest(
    [property: JsonRequired] int OrderStatus
);

