using backend_user.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace backend_user.Repositories.Interfaces;

/// <summary>
/// Unit of Work pattern interface
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IOAuthProviderRepository OAuthProviders { get; }
    IUserAnalyticsRepository UserAnalytics { get; }
    ApplicationDbContext Context { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    IExecutionStrategy CreateExecutionStrategy();
}

