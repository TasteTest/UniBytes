using backend.Common.Enums;

namespace backend.DTOs.OAuthProvider.Response;

/// <summary>
/// OAuth provider response DTO (without ID for security)
/// </summary>
public class OAuthProviderResponse
{
    public OAuthProviderType Provider { get; init; }
    public string ProviderEmail { get; init; } = string.Empty;
    public DateTime? TokenExpiresAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

