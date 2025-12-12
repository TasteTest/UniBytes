using System.Text.Json.Serialization;

namespace backend.DTOs.Loyalty.Response;

/// <summary>
/// Loyalty redemption response DTO
/// </summary>
public class LoyaltyRedemptionResponse
{
    /// <summary>Internal use only - not serialized to JSON</summary>
    [JsonIgnore]
    public Guid Id { get; init; }
    
    /// <summary>Internal use only - not serialized to JSON</summary>
    [JsonIgnore]
    public Guid LoyaltyAccountId { get; init; }
    
    public long PointsUsed { get; init; }
    public string RewardType { get; init; } = string.Empty;
    public string RewardMetadata { get; init; } = "{}";
    public DateTime CreatedAt { get; init; }
}
