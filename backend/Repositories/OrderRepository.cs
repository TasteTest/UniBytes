using backend.Data;
using backend.Models;
using backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories;

/// <summary>
/// Order repository implementation
/// </summary>
public class OrderRepository(ApplicationDbContext context) : Repository<Order>(context), IOrderRepository
{
    public override async Task<IEnumerable<Order>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(o => o.OrderItems)
            .OrderByDescending(o => o.PlacedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(o => o.UserId == userId)
            .Include(o => o.OrderItems)
            .OrderByDescending(o => o.PlacedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetByStatusAsync(int orderStatus, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(o => o.OrderStatus == orderStatus)
            .Include(o => o.OrderItems)
            .OrderByDescending(o => o.PlacedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetByExternalUserRefAsync(string externalUserRef, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(o => o.ExternalUserRef == externalUserRef)
            .Include(o => o.OrderItems)
            .OrderByDescending(o => o.PlacedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Order?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }
}
