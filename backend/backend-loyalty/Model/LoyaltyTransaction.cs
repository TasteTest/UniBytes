namespace backend_loyalty.Model;

/// <summary>
/// Loyalty transaction model for tracking points changes
/// </summary>
public class LoyaltyTransaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid LoyaltyAccountId { get; set; }
    public long ChangeAmount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public Guid? ReferenceId { get; set; }
    public string Metadata { get; set; } = "{}"; // JSONB stored as string
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public virtual LoyaltyAccount LoyaltyAccount { get; set; } = null!;
}
