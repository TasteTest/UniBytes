using AutoMapper;
using backend.Common;
using backend.DTOs.Request;
using backend.DTOs.Response;
using backend.Modelss;
using backend.Repositories.Interfaces;
using backend.Services.Interfaces;

namespace backend.Services;

/// <summary>
/// Payment service implementation
/// </summary>
public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(IPaymentRepository paymentRepository, IMapper mapper, ILogger<PaymentService> logger)
    {
        _paymentRepository = paymentRepository;
        _mapper = mapper;
        _logger = logger;
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

