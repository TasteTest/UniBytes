namespace backend_payment.Common.Enums;

/// <summary>
/// Payment status enum
/// </summary>
public enum PaymentStatus
{
    Processing = 0,
    Succeeded = 1,
    Failed = 2,
    Refunded = 3,
    Cancelled = 4
}

