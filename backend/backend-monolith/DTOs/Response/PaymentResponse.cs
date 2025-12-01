using backend_monolith.Common.Enums;

namespace backend_monolith.DTOs.Response;

/// <summary>
/// Payment response DTO
/// </summary>
public class PaymentResponse
{
    public Guid Id { get; set; }
    public Guid? OrderId { get; set; }
    public Guid? UserId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public PaymentProvider Provider { get; set; }
    public string? ProviderPaymentId { get; set; }
    public PaymentStatus Status { get; set; }
    public string? FailureMessage { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

