namespace backend.Models;

/// <summary>
/// Loyalty transaction model for tracking points changes
/// </summary>
public class LoyaltyTransaction
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid LoyaltyAccountId { get; init; }
    public long ChangeAmount { get; init; }
    public string Reason { get; init; } = string.Empty;
    public Guid? ReferenceId { get; init; }
    public string Metadata { get; init; } = "{}";
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    // Navigation property
    public virtual LoyaltyAccount LoyaltyAccount { get; init; } = null!;
}

