using backend_payment.Common;
using backend_payment.DTOs.Request;
using backend_payment.DTOs.Response;

namespace backend_payment.Services.Interfaces;

/// <summary>
/// Payment service interface
/// </summary>
public interface IPaymentService
{
    Task<Result<PaymentResponse>> GetPaymentByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<PaymentResponse>> GetPaymentByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<PaymentResponse>>> GetPaymentsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<PaymentResponse>> CreatePaymentAsync(CreatePaymentRequest request, CancellationToken cancellationToken = default);
}

