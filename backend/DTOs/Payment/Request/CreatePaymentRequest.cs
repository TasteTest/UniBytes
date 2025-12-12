using backend.Common.Enums;

namespace backend.DTOs.Payment.Request;

/// <summary>
/// Request to create a payment record
/// </summary>
public class CreatePaymentRequest
{
    public Guid? OrderId { get; init; }
    public Guid? UserId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "ron";
    public PaymentProvider Provider { get; set; }
    public string? IdempotencyKey { get; set; }
}

