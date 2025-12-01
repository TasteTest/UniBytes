using backend_monolith.Common;
using backend_monolith.Common.Enums;
using backend_monolith.DTOs.Request;
using backend_monolith.DTOs.Response;

namespace backend_monolith.Services.Interfaces;

/// <summary>
/// OAuth provider service interface
/// </summary>
public interface IOAuthProviderService
{
    Task<Result<OAuthProviderResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<OAuthProviderResponse>>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<OAuthProviderResponse>> GetByProviderAndProviderIdAsync(OAuthProviderType provider, string providerId, CancellationToken cancellationToken = default);
    Task<Result<OAuthProviderResponse>> CreateAsync(CreateOAuthProviderRequest createRequest, CancellationToken cancellationToken = default);
    Task<Result<OAuthProviderResponse>> UpdateAsync(Guid id, UpdateOAuthProviderRequest updateRequest, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
