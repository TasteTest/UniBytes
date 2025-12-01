using backend_monolith.Common.Enums;

namespace backend_monolith.DTOs.Request;

/// <summary>
/// Request to create a payment record
/// </summary>
public class CreatePaymentRequest
{
    public Guid? OrderId { get; set; }
    public Guid? UserId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public PaymentProvider Provider { get; set; }
    public string? IdempotencyKey { get; set; }
}

