using System.ComponentModel.DataAnnotations;
using backend.Common.Enums;

namespace backend.DTOs.Request;

/// <summary>
/// Request DTO for creating an OAuth provider
/// </summary>
public class CreateOAuthProviderRequest
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public OAuthProviderType Provider { get; set; }

    [Required]
    [MaxLength(255)]
    public string ProviderId { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string ProviderEmail { get; set; } = string.Empty;

    [Required]
    public string AccessToken { get; set; } = string.Empty;

    public string? RefreshToken { get; set; }

    public DateTime? TokenExpiresAt { get; set; }
}

