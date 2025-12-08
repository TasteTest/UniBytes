namespace backend.DTOs.Loyalty.Response;

/// <summary>
/// Loyalty transaction response DTO
/// </summary>
public class LoyaltyTransactionResponse
{
    public Guid Id { get; init; }
    public Guid LoyaltyAccountId { get; init; }
    public long ChangeAmount { get; init; }
    public string Reason { get; init; } = string.Empty;
    public Guid? ReferenceId { get; init; }
    public string Metadata { get; init; } = "{}";
    public DateTime CreatedAt { get; init; }
}
