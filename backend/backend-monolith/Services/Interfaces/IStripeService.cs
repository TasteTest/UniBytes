using backend_monolith.Common;
using backend_monolith.DTOs.Request;
using backend_monolith.DTOs.Response;

namespace backend_monolith.Services.Interfaces;

/// <summary>
/// Stripe service interface
/// </summary>
public interface IStripeService
{
    Task<Result<CheckoutSessionResponse>> CreateCheckoutSessionAsync(CreateCheckoutSessionRequest request, CancellationToken cancellationToken = default);
    Task<Result<PaymentResponse>> HandleWebhookEventAsync(string json, string stripeSignature, CancellationToken cancellationToken = default);
    Task<Result<PaymentResponse>> VerifyPaymentAsync(string sessionId, CancellationToken cancellationToken = default);
}

