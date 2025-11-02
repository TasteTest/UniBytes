namespace backend_payment.DTOs.Response;

/// <summary>
/// Checkout session response
/// </summary>
public class CheckoutSessionResponse
{
    public string SessionId { get; set; } = string.Empty;
    public string SessionUrl { get; set; } = string.Empty;
    public Guid PaymentId { get; set; }
    public string Message { get; set; } = string.Empty;
}

