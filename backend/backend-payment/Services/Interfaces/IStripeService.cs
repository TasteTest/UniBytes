using backend_payment.Common;
using backend_payment.DTOs.Request;
using backend_payment.DTOs.Response;

namespace backend_payment.Services.Interfaces;

/// <summary>
/// Stripe service interface
/// </summary>
public interface IStripeService
{
    Task<Result<CheckoutSessionResponse>> CreateCheckoutSessionAsync(CreateCheckoutSessionRequest request, CancellationToken cancellationToken = default);
    Task<Result<PaymentResponse>> HandleWebhookEventAsync(string json, string stripeSignature, CancellationToken cancellationToken = default);
    Task<Result<PaymentResponse>> VerifyPaymentAsync(string sessionId, CancellationToken cancellationToken = default);
}

