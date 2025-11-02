using backend_payment.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace backend_payment.Repositories.Interfaces;

/// <summary>
/// Unit of Work pattern interface
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IPaymentRepository Payments { get; }
    IIdempotencyKeyRepository IdempotencyKeys { get; }
    PaymentDbContext Context { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    IExecutionStrategy CreateExecutionStrategy();
}

