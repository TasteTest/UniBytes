using backend_monolith.Common;
using backend_monolith.Common.Enums;

namespace backend_monolith.Modelss;

/// <summary>
/// OAuth provider model for external authentication
/// </summary>
public class OAuthProvider : BaseEntity
{
    public Guid UserId { get; set; }
    public OAuthProviderType Provider { get; set; }
    public string ProviderId { get; set; } = string.Empty;
    public string ProviderEmail { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public DateTime? TokenExpiresAt { get; set; }

    // Navigation property
    public virtual User User { get; set; } = null!;
}

