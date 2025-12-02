namespace backend.DTOs.Loyalty.Response;

/// <summary>
/// Loyalty redemption response DTO
/// </summary>
public class LoyaltyRedemptionResponse
{
    public Guid Id { get; init; }
    public Guid LoyaltyAccountId { get; init; }
    public long PointsUsed { get; init; }
    public string RewardType { get; init; } = string.Empty;
    public string RewardMetadata { get; init; } = "{}";
    public DateTime CreatedAt { get; init; }
}
