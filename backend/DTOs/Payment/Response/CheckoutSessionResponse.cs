using System.Text.Json.Serialization;

namespace backend.DTOs.Payment.Response;

/// <summary>
/// Checkout session response
/// </summary>
public class CheckoutSessionResponse
{
    public string SessionId { get; set; } = string.Empty;
    public string SessionUrl { get; set; } = string.Empty;
    
    /// <summary>Internal use only - not serialized to JSON</summary>
    [JsonIgnore]
    public Guid PaymentId { get; set; }
    
    public string Message { get; set; } = string.Empty;
}
