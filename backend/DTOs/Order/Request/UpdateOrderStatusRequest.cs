namespace backend.DTOs.Order.Request;

/// <summary>
/// DTO for updating order status
/// </summary>
public record UpdateOrderStatusRequest(
    int OrderStatus
);

