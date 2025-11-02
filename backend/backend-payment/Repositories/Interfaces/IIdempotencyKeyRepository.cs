using backend_payment.Model;

namespace backend_payment.Repositories.Interfaces;

/// <summary>
/// Idempotency key repository interface
/// </summary>
public interface IIdempotencyKeyRepository : IRepository<IdempotencyKey>
{
    Task<IdempotencyKey?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<bool> KeyExistsAsync(string key, CancellationToken cancellationToken = default);
}

