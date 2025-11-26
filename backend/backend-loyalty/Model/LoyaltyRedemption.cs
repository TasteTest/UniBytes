namespace backend_loyalty.Model;

/// <summary>
/// Loyalty redemption model for tracking points usage
/// </summary>
public class LoyaltyRedemption
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid LoyaltyAccountId { get; set; }
    public long PointsUsed { get; set; }
    public string RewardType { get; set; } = string.Empty;
    public string RewardMetadata { get; set; } = "{}"; // JSONB stored as string
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public virtual LoyaltyAccount LoyaltyAccount { get; set; } = null!;
}
