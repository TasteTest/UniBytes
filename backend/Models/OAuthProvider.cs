using backend.Common;
using backend.Common.Enums;

namespace backend.Models;

/// <summary>
/// OAuth provider model for external authentication
/// </summary>
public class OAuthProvider : BaseEntity
{
    public Guid UserId { get; init; }
    public OAuthProviderType Provider { get; init; }
    public string ProviderId { get; init; } = string.Empty;
    public string ProviderEmail { get; init; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public DateTime? TokenExpiresAt { get; set; }

    // Navigation property
    public virtual User User { get; init; } = null!;
}

