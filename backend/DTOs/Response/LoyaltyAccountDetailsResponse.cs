namespace backend.DTOs.Response;

/// <summary>
/// Complete loyalty account details with history
/// </summary>
public class LoyaltyAccountDetailsResponse
{
    public LoyaltyAccountResponse Account { get; set; } = null!;
    public IEnumerable<LoyaltyTransactionResponse> RecentTransactions { get; set; } = new List<LoyaltyTransactionResponse>();
    public IEnumerable<LoyaltyRedemptionResponse> RecentRedemptions { get; set; } = new List<LoyaltyRedemptionResponse>();
    public long TotalPointsEarned { get; set; }
    public long TotalPointsRedeemed { get; set; }
}
