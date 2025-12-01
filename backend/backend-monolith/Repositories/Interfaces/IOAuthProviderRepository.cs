using backend_monolith.Common.Enums;
using backend_monolith.Modelss;

namespace backend_monolith.Repositories.Interfaces;

/// <summary>
/// OAuth provider-specific repository interface
/// </summary>
public interface IOAuthProviderRepository : IRepository<OAuthProvider>
{
    Task<IEnumerable<OAuthProvider>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<OAuthProvider?> GetByProviderAndProviderIdAsync(OAuthProviderType provider, string providerId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(OAuthProviderType provider, string providerId, CancellationToken cancellationToken = default);
}

