using backend.Models;

namespace backend.Repositories.Interfaces;

/// <summary>
/// Idempotency key repository interface
/// </summary>
public interface IIdempotencyKeyRepository : IRepository<IdempotencyKey>
{
    Task<IdempotencyKey?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<bool> KeyExistsAsync(string key, CancellationToken cancellationToken = default);
}

