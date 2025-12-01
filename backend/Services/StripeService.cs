using System.Text.Json;
using AutoMapper;
using backend.Common;
using backend.Common.Enums;
using backend.DTOs.Request;
using backend.DTOs.Response;
using backend.Modelss;
using backend.Repositories.Interfaces;
using backend.Services.Interfaces;
using Stripe;
using Stripe.Checkout;

namespace backend.Services;

/// <summary>
/// Stripe service implementation
/// </summary>
public class StripeService : IStripeService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IIdempotencyKeyRepository _idempotencyKeyRepository;
    private readonly IUserService _userService;
    private readonly IMapper _mapper;
    private readonly ILogger<StripeService> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _webhookSecret;

    public StripeService(
        IPaymentRepository paymentRepository,
        IIdempotencyKeyRepository idempotencyKeyRepository,
        IUserService userService,
        IMapper mapper,
        ILogger<StripeService> logger,
        IConfiguration configuration)
    {
        _paymentRepository = paymentRepository;
        _idempotencyKeyRepository = idempotencyKeyRepository;
        _userService = userService;
        _mapper = mapper;
        _logger = logger;
        _configuration = configuration;
        
        // Read Stripe keys from environment variables first, then configuration
        _webhookSecret = Environment.GetEnvironmentVariable("Stripe__WebhookSecret")
            ?? _configuration["Stripe:WebhookSecret"]
            ?? string.Empty;
        
        // Set Stripe API key (check environment variables first, then configuration)
        var stripeSecretKey = Environment.GetEnvironmentVariable("Stripe__SecretKey")
            ?? _configuration["Stripe:SecretKey"];
        
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

    public async Task<Result<CheckoutSessionResponse>> CreateCheckoutSessionAsync(
        CreateCheckoutSessionRequest request, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate user directly (monolith - no HTTP calls needed)
            // Get the actual User entity to access the ID
            var userEntity = await _userService.GetUserEntityByEmailAsync(request.UserEmail, cancellationToken);

            if (userEntity == null)
            {
                _logger.LogWarning("Failed to find user with email: {Email}", request.UserEmail);
                return Result<CheckoutSessionResponse>.Failure("User not found");
            }

            var userId = userEntity.Id;

            _logger.LogInformation("User authenticated: {UserId} ({Email})", userId, userEntity.Email);

            // Check for idempotency
            if (!string.IsNullOrEmpty(request.IdempotencyKey))
            {
                var keyExists = await _idempotencyKeyRepository.KeyExistsAsync(request.IdempotencyKey, cancellationToken);
                if (keyExists)
                {
                    return Result<CheckoutSessionResponse>.Failure("Duplicate request - idempotency key already used");
                }
            }

            // Create payment record with validated user ID
            var payment = new Payment
            {
                OrderId = request.OrderId,
                UserId = userId,
                Amount = request.LineItems.Sum(li => li.UnitPrice * li.Quantity),
                Currency = "usd",
                Provider = PaymentProvider.Stripe,
                Status = PaymentStatus.Processing,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _paymentRepository.AddAsync(payment, cancellationToken);

            // Create Stripe checkout session line items
            var lineItems = request.LineItems.Select(item => new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    Currency = "usd",
                    UnitAmount = (long)(item.UnitPrice * 100), // Convert to cents
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = item.Name,
                        Description = item.Description,
                        Images = !string.IsNullOrEmpty(item.ImageUrl) ? new List<string> { item.ImageUrl } : null
                    }
                },
                Quantity = item.Quantity
            }).ToList();

            // Create checkout session options
            var sessionOptions = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = lineItems,
                Mode = "payment",
                SuccessUrl = request.SuccessUrl,
                CancelUrl = request.CancelUrl,
                ClientReferenceId = payment.Id.ToString(),
                Metadata = new Dictionary<string, string>
                {
                    { "payment_id", payment.Id.ToString() },
                    { "order_id", request.OrderId.ToString() },
                    { "user_id", userId.ToString() },
                    { "user_email", userEntity.Email }
                }
            };

            // Create the session
            var service = new SessionService();
            var session = await service.CreateAsync(sessionOptions, cancellationToken: cancellationToken);

            // Update payment with Stripe session ID
            payment.ProviderPaymentId = session.Id;
            payment.UpdatedAt = DateTime.UtcNow;
            await _paymentRepository.UpdateAsync(payment, cancellationToken);

            // Save idempotency key if provided
            if (!string.IsNullOrEmpty(request.IdempotencyKey))
            {
                var idempotencyKey = new IdempotencyKey
                {
                    Key = request.IdempotencyKey,
                    UserId = userId,
                    ExpiresAt = DateTime.UtcNow.AddHours(24),
                    CreatedAt = DateTime.UtcNow
                };
                await _idempotencyKeyRepository.AddAsync(idempotencyKey, cancellationToken);
            }

            var response = new CheckoutSessionResponse
            {
                SessionId = session.Id,
                SessionUrl = session.Url,
                PaymentId = payment.Id,
                Message = "Checkout session created successfully"
            };

            _logger.LogInformation("Created Stripe checkout session {SessionId} for order {OrderId}", session.Id, request.OrderId);
            return Result<CheckoutSessionResponse>.Success(response);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error creating checkout session for order {OrderId}", request.OrderId);
            return Result<CheckoutSessionResponse>.Failure($"Stripe error: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating checkout session for order {OrderId}", request.OrderId);
            return Result<CheckoutSessionResponse>.Failure($"Error creating checkout session: {ex.Message}");
        }
    }

    public async Task<Result<PaymentResponse>> HandleWebhookEventAsync(
        string json, 
        string stripeSignature, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, _webhookSecret);

            _logger.LogInformation("Processing Stripe webhook event: {EventType}", stripeEvent.Type);

            switch (stripeEvent.Type)
            {
                case "checkout.session.completed":
                    var session = stripeEvent.Data.Object as Session;
                    if (session != null)
                    {
                        return await ProcessCheckoutSessionCompletedAsync(session, cancellationToken);
                    }
                    break;

                case "payment_intent.succeeded":
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    if (paymentIntent != null)
                    {
                        return await ProcessPaymentIntentSucceededAsync(paymentIntent, cancellationToken);
                    }
                    break;

                case "payment_intent.payment_failed":
                    var failedPaymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    if (failedPaymentIntent != null)
                    {
                        return await ProcessPaymentIntentFailedAsync(failedPaymentIntent, cancellationToken);
                    }
                    break;

                default:
                    _logger.LogInformation("Unhandled Stripe webhook event type: {EventType}", stripeEvent.Type);
                    break;
            }

            return Result<PaymentResponse>.Success(null!);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe webhook signature verification failed");
            return Result<PaymentResponse>.Failure($"Webhook verification failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook event");
            return Result<PaymentResponse>.Failure($"Error processing webhook: {ex.Message}");
        }
    }

    public async Task<Result<PaymentResponse>> VerifyPaymentAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var sessionService = new SessionService();
            var session = await sessionService.GetAsync(sessionId, cancellationToken: cancellationToken);

            var payment = await _paymentRepository.GetByProviderPaymentIdAsync(sessionId, cancellationToken);
            if (payment == null)
            {
                return Result<PaymentResponse>.Failure("Payment not found");
            }

            // Update payment status based on session
            if (session.PaymentStatus == "paid")
            {
                payment.Status = PaymentStatus.Succeeded;
                payment.ProviderChargeId = session.PaymentIntentId;
            }
            else
            {
                payment.Status = PaymentStatus.Failed;
                payment.FailureMessage = $"Payment status: {session.PaymentStatus}";
            }

            payment.RawProviderResponse = JsonSerializer.Serialize(session);
            payment.UpdatedAt = DateTime.UtcNow;

            await _paymentRepository.UpdateAsync(payment, cancellationToken);

            var paymentResponse = _mapper.Map<PaymentResponse>(payment);
            return Result<PaymentResponse>.Success(paymentResponse);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error verifying payment session {SessionId}", sessionId);
            return Result<PaymentResponse>.Failure($"Stripe error: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying payment session {SessionId}", sessionId);
            return Result<PaymentResponse>.Failure($"Error verifying payment: {ex.Message}");
        }
    }

    private async Task<Result<PaymentResponse>> ProcessCheckoutSessionCompletedAsync(
        Session session, 
        CancellationToken cancellationToken)
    {
        try
        {
            var paymentId = session.ClientReferenceId;
            if (string.IsNullOrEmpty(paymentId) || !Guid.TryParse(paymentId, out var paymentGuid))
            {
                _logger.LogWarning("Invalid payment ID in checkout session {SessionId}", session.Id);
                return Result<PaymentResponse>.Failure("Invalid payment ID");
            }

            var payment = await _paymentRepository.GetByIdAsync(paymentGuid, cancellationToken);
            if (payment == null)
            {
                return Result<PaymentResponse>.Failure("Payment not found");
            }

            payment.Status = PaymentStatus.Succeeded;
            payment.ProviderChargeId = session.PaymentIntentId;
            payment.RawProviderResponse = JsonSerializer.Serialize(session);
            payment.UpdatedAt = DateTime.UtcNow;

            await _paymentRepository.UpdateAsync(payment, cancellationToken);

            _logger.LogInformation("Payment {PaymentId} marked as succeeded", payment.Id);

            var paymentResponse = _mapper.Map<PaymentResponse>(payment);
            return Result<PaymentResponse>.Success(paymentResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing checkout session completed");
            return Result<PaymentResponse>.Failure($"Error processing payment: {ex.Message}");
        }
    }

    private async Task<Result<PaymentResponse>> ProcessPaymentIntentSucceededAsync(
        PaymentIntent paymentIntent, 
        CancellationToken cancellationToken)
    {
        try
        {
            var payment = await _paymentRepository
                .FirstOrDefaultAsync(p => p.ProviderChargeId == paymentIntent.Id, cancellationToken);

            if (payment != null)
            {
                payment.Status = PaymentStatus.Succeeded;
                payment.RawProviderResponse = JsonSerializer.Serialize(paymentIntent);
                payment.UpdatedAt = DateTime.UtcNow;

                await _paymentRepository.UpdateAsync(payment, cancellationToken);

                var paymentResponse = _mapper.Map<PaymentResponse>(payment);
                return Result<PaymentResponse>.Success(paymentResponse);
            }

            return Result<PaymentResponse>.Success(null!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment intent succeeded");
            return Result<PaymentResponse>.Failure($"Error processing payment intent: {ex.Message}");
        }
    }

    private async Task<Result<PaymentResponse>> ProcessPaymentIntentFailedAsync(
        PaymentIntent paymentIntent, 
        CancellationToken cancellationToken)
    {
        try
        {
            var payment = await _paymentRepository
                .FirstOrDefaultAsync(p => p.ProviderChargeId == paymentIntent.Id, cancellationToken);

            if (payment != null)
            {
                payment.Status = PaymentStatus.Failed;
                payment.FailureMessage = paymentIntent.LastPaymentError?.Message ?? "Payment failed";
                payment.RawProviderResponse = JsonSerializer.Serialize(paymentIntent);
                payment.UpdatedAt = DateTime.UtcNow;

                await _paymentRepository.UpdateAsync(payment, cancellationToken);

                var paymentResponse = _mapper.Map<PaymentResponse>(payment);
                return Result<PaymentResponse>.Success(paymentResponse);
            }

            return Result<PaymentResponse>.Success(null!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment intent failed");
            return Result<PaymentResponse>.Failure($"Error processing payment intent: {ex.Message}");
        }
    }
}

