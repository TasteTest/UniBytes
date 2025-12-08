using backend.Models;

namespace backend.Repositories.Interfaces;

/// <summary>
/// Order-specific repository interface
/// </summary>
public interface IOrderRepository : IRepository<Order>
{
    /// <summary>
    /// Get orders by user ID
    /// </summary>
    Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get orders by status
    /// </summary>
    Task<IEnumerable<Order>> GetByStatusAsync(int orderStatus, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get orders by external user reference
    /// </summary>
    Task<IEnumerable<Order>> GetByExternalUserRefAsync(string externalUserRef, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get order with items
    /// </summary>
    Task<Order?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default);
}

