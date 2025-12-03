using backend.Data;
using backend.Models;
using backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories;

/// <summary>
/// OrderItem repository implementation
/// </summary>
public class OrderItemRepository(ApplicationDbContext context) : Repository<OrderItem>(context), IOrderItemRepository
{
    public async Task<IEnumerable<OrderItem>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(oi => oi.OrderId == orderId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<OrderItem>> GetByMenuItemIdAsync(Guid menuItemId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(oi => oi.MenuItemId == menuItemId)
            .ToListAsync(cancellationToken);
    }
}

