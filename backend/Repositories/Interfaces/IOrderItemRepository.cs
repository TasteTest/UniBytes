using backend.Models;

namespace backend.Repositories.Interfaces;

/// <summary>
/// OrderItem-specific repository interface
/// </summary>
public interface IOrderItemRepository : IRepository<OrderItem>
{
    /// <summary>
    /// Get order items by order ID
    /// </summary>
    Task<IEnumerable<OrderItem>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get order items by menu item ID
    /// </summary>
    Task<IEnumerable<OrderItem>> GetByMenuItemIdAsync(Guid menuItemId, CancellationToken cancellationToken = default);
}

