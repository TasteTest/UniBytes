using backend.Common;
using backend.Common.Enums;

namespace backend.Modelss;

/// <summary>
/// Payment entity
/// </summary>
public class Payment : BaseEntity
{
    public Guid? OrderId { get; set; }
    public Guid? UserId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public PaymentProvider Provider { get; set; }
    public string? ProviderPaymentId { get; set; }
    public string? ProviderChargeId { get; set; }
    public PaymentStatus Status { get; set; }
    public string? RawProviderResponse { get; set; }
    public string? FailureMessage { get; set; }
}

