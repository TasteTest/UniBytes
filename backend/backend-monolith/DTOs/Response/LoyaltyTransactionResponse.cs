namespace backend_monolith.DTOs.Response;

/// <summary>
/// Loyalty transaction response DTO
/// </summary>
public class LoyaltyTransactionResponse
{
    public Guid Id { get; set; }
    public Guid LoyaltyAccountId { get; set; }
    public long ChangeAmount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public Guid? ReferenceId { get; set; }
    public string Metadata { get; set; } = "{}";
    public DateTime CreatedAt { get; set; }
}
