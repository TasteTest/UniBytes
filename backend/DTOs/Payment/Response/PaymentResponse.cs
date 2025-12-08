using backend.Common.Enums;

namespace backend.DTOs.Payment.Response;

/// <summary>
/// Payment response DTO
/// </summary>
public class PaymentResponse
{
    public Guid Id { get; init; }
    public Guid? OrderId { get; init; }
    public Guid? UserId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "USD";
    public PaymentProvider Provider { get; init; }
    public string? ProviderPaymentId { get; init; }
    public PaymentStatus Status { get; init; }
    public string? FailureMessage { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

