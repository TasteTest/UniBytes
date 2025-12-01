using backend_monolith.Common;
using backend_monolith.DTOs.Request;
using backend_monolith.DTOs.Response;

namespace backend_monolith.Services.Interfaces;

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

