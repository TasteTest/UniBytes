using backend.Data;
using backend.Modelss;
using backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories;

/// <summary>
/// Idempotency key repository implementation
/// </summary>
public class IdempotencyKeyRepository : Repository<IdempotencyKey>, IIdempotencyKeyRepository
{
    public IdempotencyKeyRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IdempotencyKey?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(ik => ik.Key == key, cancellationToken);
    }

    public async Task<bool> KeyExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(ik => ik.Key == key && (ik.ExpiresAt == null || ik.ExpiresAt > DateTimeOffset.UtcNow), cancellationToken);
    }
}

