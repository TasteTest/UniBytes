using backend.Common.Enums;

namespace backend.DTOs.Loyalty.Response;

/// <summary>
/// Loyalty account response DTO
/// </summary>
public class LoyaltyAccountResponse
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public long PointsBalance { get; init; }
    public LoyaltyTier Tier { get; init; }
    public string TierName { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
