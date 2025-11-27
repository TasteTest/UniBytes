using backend_loyalty.Common;
using backend_loyalty.Common.Enums;

namespace backend_loyalty.Model;

/// <summary>
/// Loyalty account model matching the database schema
/// </summary>
public class LoyaltyAccount : BaseEntity
{
    public Guid UserId { get; set; }
    public long PointsBalance { get; set; } = 0;
    public LoyaltyTier Tier { get; set; } = LoyaltyTier.Bronze;
    public bool IsActive { get; set; } = true;

    
    public virtual ICollection<LoyaltyTransaction> LoyaltyTransactions { get; set; } = new List<LoyaltyTransaction>();
    public virtual ICollection<LoyaltyRedemption> LoyaltyRedemptions { get; set; } = new List<LoyaltyRedemption>();
}
