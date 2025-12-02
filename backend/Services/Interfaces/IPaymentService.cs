using backend.Common;
using backend.DTOs.Payment.Request;
using backend.DTOs.Payment.Response;

namespace backend.Services.Interfaces;

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

