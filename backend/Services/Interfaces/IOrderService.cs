using backend.Common;
using backend.Models;
using backend.DTOs.Order.Request;
using backend.DTOs.Order.Response;

namespace backend.Services.Interfaces;

/// <summary>
/// Order service interface for managing orders
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// Get order by ID
    /// </summary>
    Task<Result<OrderResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all orders
    /// </summary>
    Task<Result<IEnumerable<OrderResponse>>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get orders by user ID
    /// </summary>
    Task<Result<IEnumerable<OrderResponse>>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get orders by status
    /// </summary>
    Task<Result<IEnumerable<OrderResponse>>> GetByStatusAsync(int orderStatus, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new order
    /// </summary>
    Task<Result<OrderResponse>> CreateAsync(CreateOrderRequest createRequest, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update order status
    /// </summary>
    Task<Result<OrderResponse>> UpdateStatusAsync(Guid id, UpdateOrderStatusRequest updateRequest, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancel an order
    /// </summary>
    Task<Result<OrderResponse>> CancelAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete an order
    /// </summary>
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get order by external user reference
    /// </summary>
    Task<Result<IEnumerable<OrderResponse>>> GetByExternalUserRefAsync(string externalUserRef, CancellationToken cancellationToken = default);
}

