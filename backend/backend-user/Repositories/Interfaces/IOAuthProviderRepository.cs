using backend_user.Common.Enums;
using backend_user.Model;

namespace backend_user.Repositories.Interfaces;

/// <summary>
/// OAuth provider-specific repository interface
/// </summary>
public interface IOAuthProviderRepository : IRepository<OAuthProvider>
{
    Task<IEnumerable<OAuthProvider>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<OAuthProvider?> GetByProviderAndProviderIdAsync(OAuthProviderType provider, string providerId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(OAuthProviderType provider, string providerId, CancellationToken cancellationToken = default);
}

