namespace backend.DTOs.Loyalty.Response;

/// <summary>
/// Complete loyalty account details with history
/// </summary>
public class LoyaltyAccountDetailsResponse
{
    public LoyaltyAccountResponse Account { get; init; } = null!;
    public IEnumerable<LoyaltyTransactionResponse> RecentTransactions { get; init; } = new List<LoyaltyTransactionResponse>();
    public IEnumerable<LoyaltyRedemptionResponse> RecentRedemptions { get; init; } = new List<LoyaltyRedemptionResponse>();
    public long TotalPointsEarned { get; init; }
    public long TotalPointsRedeemed { get; init; }
}
