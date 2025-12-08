using backend.Common;
using backend.Common.Enums;
using backend.DTOs.OAuthProvider.Request;
using backend.DTOs.OAuthProvider.Response;

namespace backend.Services.Interfaces;

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
