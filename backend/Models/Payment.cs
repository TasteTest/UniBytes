using backend.Common;
using backend.Common.Enums;

namespace backend.Models;

/// <summary>
/// Payment entity
/// </summary>
public class Payment : BaseEntity
{
    public Guid? OrderId { get; init; }
    public Guid? UserId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "ron";
    public PaymentProvider Provider { get; init; }
    public string? ProviderPaymentId { get; set; }
    public string? ProviderChargeId { get; set; }
    public PaymentStatus Status { get; set; }
    public string? RawProviderResponse { get; set; }
    public string? FailureMessage { get; set; }
}

