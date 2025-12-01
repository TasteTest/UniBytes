using backend.Common.Enums;
using backend.Modelss;

namespace backend.Repositories.Interfaces;

/// <summary>
/// OAuth provider-specific repository interface
/// </summary>
public interface IOAuthProviderRepository : IRepository<OAuthProvider>
{
    Task<IEnumerable<OAuthProvider>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<OAuthProvider?> GetByProviderAndProviderIdAsync(OAuthProviderType provider, string providerId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(OAuthProviderType provider, string providerId, CancellationToken cancellationToken = default);
}

