namespace backend_user.DTOs.Request;

/// <summary>
/// Request DTO for updating an OAuth provider
/// </summary>
public class UpdateOAuthProviderRequest
{
    public string? AccessToken { get; set; }

    public string? RefreshToken { get; set; }

    public DateTime? TokenExpiresAt { get; set; }
}

