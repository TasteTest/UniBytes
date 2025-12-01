using backend_monolith.Common.Enums;

namespace backend_monolith.DTOs.Response;

/// <summary>
/// OAuth provider response DTO (without ID for security)
/// </summary>
public class OAuthProviderResponse
{
    public OAuthProviderType Provider { get; set; }
    public string ProviderEmail { get; set; } = string.Empty;
    public DateTime? TokenExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

