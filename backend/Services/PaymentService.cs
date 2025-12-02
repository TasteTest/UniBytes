using AutoMapper;
using backend.Common;
using backend.Models;
using backend.Repositories.Interfaces;
using backend.Services.Interfaces;
using backend.DTOs.Payment.Request;
using backend.DTOs.Payment.Response;

namespace backend.Services;

/// <summary>
/// Payment service implementation
/// </summary>
public class PaymentService(IPaymentRepository paymentRepository, IMapper mapper, ILogger<PaymentService> logger)
    : IPaymentService
{
    public async Task<Result<PaymentResponse>> GetPaymentByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var payment = await paymentRepository.GetByIdAsync(id, cancellationToken);
            if (payment == null)
            {
                return Result<PaymentResponse>.Failure("Payment not found");
            }

            var paymentResponse = mapper.Map<PaymentResponse>(payment);
            return Result<PaymentResponse>.Success(paymentResponse);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting payment by ID {PaymentId}", id);
            return Result<PaymentResponse>.Failure($"Error retrieving payment: {ex.Message}");
        }
    }

    public async Task<Result<PaymentResponse>> GetPaymentByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        try
        {
            var payment = await paymentRepository.GetByOrderIdAsync(orderId, cancellationToken);
            if (payment == null)
            {
                return Result<PaymentResponse>.Failure("Payment not found for order");
            }

            var paymentResponse = mapper.Map<PaymentResponse>(payment);
            return Result<PaymentResponse>.Success(paymentResponse);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting payment by order ID {OrderId}", orderId);
            return Result<PaymentResponse>.Failure($"Error retrieving payment: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<PaymentResponse>>> GetPaymentsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var payments = await paymentRepository.GetByUserIdAsync(userId, cancellationToken);
            var paymentResponses = mapper.Map<IEnumerable<PaymentResponse>>(payments);
            return Result<IEnumerable<PaymentResponse>>.Success(paymentResponses);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting payments by user ID {UserId}", userId);
            return Result<IEnumerable<PaymentResponse>>.Failure($"Error retrieving payments: {ex.Message}");
        }
    }

    public async Task<Result<PaymentResponse>> CreatePaymentAsync(CreatePaymentRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var payment = mapper.Map<Payment>(request);
            payment.CreatedAt = DateTime.UtcNow;
            payment.UpdatedAt = DateTime.UtcNow;

            await paymentRepository.AddAsync(payment, cancellationToken);

            var paymentResponse = mapper.Map<PaymentResponse>(payment);
            return Result<PaymentResponse>.Success(paymentResponse);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating payment for order {OrderId}", request.OrderId);
            return Result<PaymentResponse>.Failure($"Error creating payment: {ex.Message}");
        }
    }
}

