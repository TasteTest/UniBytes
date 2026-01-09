using Stripe;
using Stripe.Checkout;
using backend.Services.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace backend.Services.Wrappers;

/// <summary>
/// Wrapper around Stripe SDK for testability
/// </summary>
[ExcludeFromCodeCoverage]
public class StripeServiceWrapper : IStripeServiceWrapper
{
    private readonly string _webhookSecret;
    private readonly ILogger<StripeServiceWrapper> _logger;

    public StripeServiceWrapper(IConfiguration configuration, ILogger<StripeServiceWrapper> logger)
    {
        _logger = logger;
        
        // Set Stripe API key (check environment variables first, then configuration)
        var stripeSecretKey = Environment.GetEnvironmentVariable("Stripe__SecretKey")
            ?? configuration["Stripe:SecretKey"];
        
        // Read webhook secret
        _webhookSecret = Environment.GetEnvironmentVariable("Stripe__WebhookSecret")
                          ?? configuration["Stripe:WebhookSecret"]
                          ?? string.Empty;
        
        if (string.IsNullOrEmpty(stripeSecretKey))
        {
            _logger.LogWarning("Stripe secret key not configured. Please set Stripe__SecretKey environment variable or Stripe:SecretKey in appsettings.");
        }
        else
        {
            StripeConfiguration.ApiKey = stripeSecretKey;
            _logger.LogInformation("Stripe API key configured (last 10 chars: {LastChars})", stripeSecretKey.Substring(Math.Max(0, stripeSecretKey.Length - 10)));
        }
    }

    public async Task<Session> CreateCheckoutSessionAsync(SessionCreateOptions options, CancellationToken cancellationToken = default)
    {
        var service = new SessionService();
        return await service.CreateAsync(options, cancellationToken: cancellationToken);
    }

    public async Task<Session> GetCheckoutSessionAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        var service = new SessionService();
        return await service.GetAsync(sessionId, cancellationToken: cancellationToken);
    }

    public Event ConstructWebhookEvent(string json, string stripeSignature)
    {
        return EventUtility.ConstructEvent(json, stripeSignature, _webhookSecret);
    }
}
