using backend_payment.Common;

namespace backend_payment.Model;

/// <summary>
/// Idempotency key entity for safe retries
/// </summary>
public class IdempotencyKey : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
}

