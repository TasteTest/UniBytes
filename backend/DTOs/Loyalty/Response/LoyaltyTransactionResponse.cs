using System.Text.Json.Serialization;

namespace backend.DTOs.Loyalty.Response;

/// <summary>
/// Loyalty transaction response DTO
/// </summary>
public class LoyaltyTransactionResponse
{
    /// <summary>Internal use only - not serialized to JSON</summary>
    [JsonIgnore]
    public Guid Id { get; init; }
    
    /// <summary>Internal use only - not serialized to JSON</summary>
    [JsonIgnore]
    public Guid LoyaltyAccountId { get; init; }
    
    public long ChangeAmount { get; init; }
    public string Reason { get; init; } = string.Empty;
    
    /// <summary>Internal use only - not serialized to JSON</summary>
    [JsonIgnore]
    public Guid? ReferenceId { get; init; }
    
    public string Metadata { get; init; } = "{}";
    public DateTime CreatedAt { get; init; }
}
