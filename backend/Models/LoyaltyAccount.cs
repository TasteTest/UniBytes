using backend.Common;
using backend.Common.Enums;

namespace backend.Models;

/// <summary>
/// Loyalty account model
/// </summary>
public class LoyaltyAccount : BaseEntity
{
    public Guid UserId { get; set; }
    public long PointsBalance { get; set; } = 0;
    public LoyaltyTier Tier { get; set; } = LoyaltyTier.Bronze;
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<LoyaltyTransaction> LoyaltyTransactions { get; set; } = new List<LoyaltyTransaction>();
    public virtual ICollection<LoyaltyRedemption> LoyaltyRedemptions { get; set; } = new List<LoyaltyRedemption>();
}

