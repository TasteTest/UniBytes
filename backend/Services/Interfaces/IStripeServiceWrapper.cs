using Stripe;
using Stripe.Checkout;

namespace backend.Services.Interfaces;

/// <summary>
/// Wrapper interface for Stripe API operations to enable testability
/// </summary>
public interface IStripeServiceWrapper
{
    /// <summary>
    /// Create a Stripe checkout session
    /// </summary>
    Task<Session> CreateCheckoutSessionAsync(SessionCreateOptions options, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get a checkout session by ID
    /// </summary>
    Task<Session> GetCheckoutSessionAsync(string sessionId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Construct and validate a Stripe webhook event
    /// </summary>
    Event ConstructWebhookEvent(string json, string stripeSignature, string webhookSecret);
}
