namespace backend.Models;

/// <summary>
/// Loyalty redemption model for tracking points usage
/// </summary>
public class LoyaltyRedemption
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid LoyaltyAccountId { get; init; }
    public long PointsUsed { get; init; }
    public string RewardType { get; init; } = string.Empty;
    public string RewardMetadata { get; init; } = "{}";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public virtual LoyaltyAccount LoyaltyAccount { get; set; } = null!;
}

