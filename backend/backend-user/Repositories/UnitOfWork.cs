using backend_user.Data;
using backend_user.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace backend_user.Repositories;

/// <summary>
/// Unit of Work pattern implementation
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    public IUserRepository Users { get; }
    public IOAuthProviderRepository OAuthProviders { get; }
    public IUserAnalyticsRepository UserAnalytics { get; }
    public ApplicationDbContext Context => _context;

    public UnitOfWork(
        ApplicationDbContext context,
        IUserRepository userRepository,
        IOAuthProviderRepository oAuthProviderRepository,
        IUserAnalyticsRepository userAnalyticsRepository)
    {
        _context = context;
        Users = userRepository;
        OAuthProviders = oAuthProviderRepository;
        UserAnalytics = userAnalyticsRepository;
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

