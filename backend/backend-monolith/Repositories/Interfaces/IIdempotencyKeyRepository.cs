using backend_monolith.Modelss;

namespace backend_monolith.Repositories.Interfaces;

/// <summary>
/// Idempotency key repository interface
/// </summary>
public interface IIdempotencyKeyRepository : IRepository<IdempotencyKey>
{
    Task<IdempotencyKey?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<bool> KeyExistsAsync(string key, CancellationToken cancellationToken = default);
}

