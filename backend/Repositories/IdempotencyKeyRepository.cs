using backend.Data;
using backend.Models;
using backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories;

/// <summary>
/// Idempotency key repository implementation
/// </summary>
public class IdempotencyKeyRepository(ApplicationDbContext context)
    : Repository<IdempotencyKey>(context), IIdempotencyKeyRepository
{
    public async Task<IdempotencyKey?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(ik => ik.Key == key, cancellationToken);
    }

    public async Task<bool> KeyExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AnyAsync(ik => ik.Key == key && (ik.ExpiresAt == null || ik.ExpiresAt > DateTimeOffset.UtcNow), cancellationToken);
    }
}

