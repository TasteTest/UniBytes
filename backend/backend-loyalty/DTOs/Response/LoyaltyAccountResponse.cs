using backend_loyalty.Common.Enums;

namespace backend_loyalty.DTOs.Response;

/// <summary>
/// Loyalty account response DTO
/// </summary>
public class LoyaltyAccountResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public long PointsBalance { get; set; }
    public LoyaltyTier Tier { get; set; }
    public string TierName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
