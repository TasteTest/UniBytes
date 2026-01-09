using AutoMapper;
using backend.Common;
using backend.Models;
using backend.Repositories.Interfaces;
using backend.Services.Interfaces;
using backend.DTOs.Payment.Request;
using backend.DTOs.Payment.Response;
using Stripe.Checkout;

namespace backend.Services;

/// <summary>
/// Payment service implementation
/// </summary>
public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<PaymentService> _logger;
    private readonly IStripeServiceWrapper _stripeWrapper;
    private readonly string _frontendUrl;

    // Use empty string as default, or a placeholder that indicates configuration is missing
    private const string DefaultFrontendUrl = "";

    public PaymentService(
        IPaymentRepository paymentRepository, 
        IMapper mapper, 
        ILogger<PaymentService> logger,
        IStripeServiceWrapper stripeWrapper,
        IConfiguration configuration)
    {
        _paymentRepository = paymentRepository;
        _mapper = mapper;
        _logger = logger;
        _stripeWrapper = stripeWrapper;
        _frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL") 
                       ?? configuration["FrontendUrl"] 
                       ?? DefaultFrontendUrl;
    }

    public async Task<Result<PaymentResponse>> GetPaymentByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var payment = await _paymentRepository.GetByIdAsync(id, cancellationToken);
            if (payment == null)
            {
                return Result<PaymentResponse>.Failure("Payment not found");
            }

            var paymentResponse = _mapper.Map<PaymentResponse>(payment);
            return Result<PaymentResponse>.Success(paymentResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment by ID {PaymentId}", id);
            return Result<PaymentResponse>.Failure($"Error retrieving payment: {ex.Message}");
        }
    }

    public async Task<Result<PaymentResponse>> GetPaymentByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        try
        {
            var payment = await _paymentRepository.GetByOrderIdAsync(orderId, cancellationToken);
            if (payment == null)
            {
                return Result<PaymentResponse>.Failure("Payment not found for order");
            }

            var paymentResponse = _mapper.Map<PaymentResponse>(payment);
            return Result<PaymentResponse>.Success(paymentResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment by order ID {OrderId}", orderId);
            return Result<PaymentResponse>.Failure($"Error retrieving payment: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<PaymentResponse>>> GetPaymentsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var payments = await _paymentRepository.GetByUserIdAsync(userId, cancellationToken);
            var paymentResponses = _mapper.Map<IEnumerable<PaymentResponse>>(payments);
            return Result<IEnumerable<PaymentResponse>>.Success(paymentResponses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payments by user ID {UserId}", userId);
            return Result<IEnumerable<PaymentResponse>>.Failure($"Error retrieving payments: {ex.Message}");
        }
    }
    
    public async Task<Result<CheckoutSessionResponse>> CreateCheckoutSessionAsync(CreateCheckoutSessionRequest request, string userId)
    {
        try
        {
            var lineItems = new List<SessionLineItemOptions>();

            foreach (var item in request.LineItems)
            {
                lineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.UnitPrice * 100),
                        Currency = "ron",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Name,
                        },
                    },
                    Quantity = item.Quantity,
                });
            }

            var domain = _frontendUrl;
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = lineItems,
                Mode = "payment",
                SuccessUrl = domain + "/success?session_id={CHECKOUT_SESSION_ID}",
                CancelUrl = domain + "/checkout",
                CustomerEmail = request.UserEmail,
                Metadata = new Dictionary<string, string>
                {
                    { "userId", userId },
                    { "customOrderId", Guid.NewGuid().ToString() }
                }
            };

            var session = await _stripeWrapper.CreateCheckoutSessionAsync(options);

            return Result<CheckoutSessionResponse>.Success(new CheckoutSessionResponse 
            { 
                SessionId = session.Id, 
                SessionUrl = session.Url
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Stripe session for user {UserId}", userId);
            return Result<CheckoutSessionResponse>.Failure(ex.Message);
        }
    }

    public async Task<Result<PaymentResponse>> CreatePaymentAsync(CreatePaymentRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var payment = _mapper.Map<Payment>(request);
            payment.CreatedAt = DateTime.UtcNow;
            payment.UpdatedAt = DateTime.UtcNow;

            await _paymentRepository.AddAsync(payment, cancellationToken);

            var paymentResponse = _mapper.Map<PaymentResponse>(payment);
            return Result<PaymentResponse>.Success(paymentResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment for order {OrderId}", request.OrderId);
            return Result<PaymentResponse>.Failure($"Error creating payment: {ex.Message}");
        }
    }
}

