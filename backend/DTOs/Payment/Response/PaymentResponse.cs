using System.Text.Json.Serialization;
using backend.Common.Enums;

namespace backend.DTOs.Payment.Response;

/// <summary>
/// Payment response DTO
/// </summary>
public class PaymentResponse
{
    /// <summary>Internal use only - not serialized to JSON</summary>
    [JsonIgnore]
    public Guid Id { get; init; }
    
    /// <summary>Internal use only - not serialized to JSON</summary>
    [JsonIgnore]
    public Guid? OrderId { get; init; }
    
    /// <summary>Internal use only - not serialized to JSON</summary>
    [JsonIgnore]
    public Guid? UserId { get; init; }
    
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "ron";
    public PaymentProvider Provider { get; init; }
    public string? ProviderPaymentId { get; init; }
    public PaymentStatus Status { get; init; }
    public string? FailureMessage { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

