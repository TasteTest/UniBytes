using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using backend.Common.Enums;

namespace backend.DTOs.OAuthProvider.Request;

/// <summary>
/// Request DTO for creating an OAuth provider
/// </summary>
public class CreateOAuthProviderRequest
{
    [Required]
    [JsonRequired]
    public Guid UserId { get; init; }

    [Required]
    [JsonRequired]
    public OAuthProviderType Provider { get; init; }

    [Required]
    [MaxLength(255)]
    public string ProviderId { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string ProviderEmail { get; init; } = string.Empty;

    [Required]
    public string AccessToken { get; set; } = string.Empty;

    public string? RefreshToken { get; set; }

    public DateTime? TokenExpiresAt { get; set; }
}

