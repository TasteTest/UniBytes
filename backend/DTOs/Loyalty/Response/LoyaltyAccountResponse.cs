using System.Text.Json.Serialization;
using backend.Common.Enums;

namespace backend.DTOs.Loyalty.Response;

/// <summary>
/// Loyalty account response DTO
/// </summary>
public class LoyaltyAccountResponse
{
    /// <summary>Internal use only - not serialized to JSON</summary>
    [JsonIgnore]
    public Guid Id { get; init; }
    
    /// <summary>Internal use only - not serialized to JSON</summary>
    [JsonIgnore]
    public Guid UserId { get; init; }
    
    public long PointsBalance { get; init; }
    public LoyaltyTier Tier { get; init; }
    public string TierName { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
