using backend_payment.Data;
using backend_payment.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace backend_payment.Repositories;

/// <summary>
/// Unit of Work pattern implementation
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly PaymentDbContext _context;
    private IDbContextTransaction? _transaction;

    public IPaymentRepository Payments { get; }
    public IIdempotencyKeyRepository IdempotencyKeys { get; }
    public PaymentDbContext Context => _context;

    public UnitOfWork(
        PaymentDbContext context,
        IPaymentRepository paymentRepository,
        IIdempotencyKeyRepository idempotencyKeyRepository)
    {
        _context = context;
        Payments = paymentRepository;
        IdempotencyKeys = idempotencyKeyRepository;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public IExecutionStrategy CreateExecutionStrategy()
    {
        return _context.Database.CreateExecutionStrategy();
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}

