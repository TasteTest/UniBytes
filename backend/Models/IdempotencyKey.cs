using backend.Common;

namespace backend.Models;

/// <summary>
/// Idempotency key entity for safe retries
/// </summary>
public class IdempotencyKey : BaseEntity
{
    public string Key { get; init; } = string.Empty;
    public Guid? UserId { get; init; }
    public DateTimeOffset? ExpiresAt { get; init; }
}

