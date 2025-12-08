using backend.Common;
using backend.DTOs.Payment.Request;
using backend.DTOs.Payment.Response;

namespace backend.Services.Interfaces;

/// <summary>
/// Stripe service interface
/// </summary>
public interface IStripeService
{
    Task<Result<CheckoutSessionResponse>> CreateCheckoutSessionAsync(CreateCheckoutSessionRequest request, CancellationToken cancellationToken = default);
    Task<Result<PaymentResponse>> HandleWebhookEventAsync(string json, string stripeSignature, CancellationToken cancellationToken = default);
    Task<Result<PaymentResponse>> VerifyPaymentAsync(string sessionId, CancellationToken cancellationToken = default);
}

