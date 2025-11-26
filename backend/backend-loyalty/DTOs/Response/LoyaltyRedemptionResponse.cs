namespace backend_loyalty.DTOs.Response;

/// <summary>
/// Loyalty redemption response DTO
/// </summary>
public class LoyaltyRedemptionResponse
{
    public Guid Id { get; set; }
    public Guid LoyaltyAccountId { get; set; }
    public long PointsUsed { get; set; }
    public string RewardType { get; set; } = string.Empty;
    public string RewardMetadata { get; set; } = "{}";
    public DateTime CreatedAt { get; set; }
}
