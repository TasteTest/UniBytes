using backend_payment.Data;
using backend_payment.Model;
using backend_payment.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend_payment.Repositories;

/// <summary>
/// Idempotency key repository implementation
/// </summary>
public class IdempotencyKeyRepository : Repository<IdempotencyKey>, IIdempotencyKeyRepository
{
    public IdempotencyKeyRepository(PaymentDbContext context) : base(context)
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

