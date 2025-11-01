using System.ComponentModel.DataAnnotations;
using backend_user.Common.Enums;

namespace backend_user.DTOs.Request;

/// <summary>
/// Request DTO for Google OAuth authentication
/// </summary>
public class GoogleAuthRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? AvatarUrl { get; set; }

    [Required]
    public OAuthProviderType Provider { get; set; }

    [Required]
    public string ProviderId { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string ProviderEmail { get; set; } = string.Empty;

    [Required]
    public string AccessToken { get; set; } = string.Empty;

    public string? RefreshToken { get; set; }

    public DateTime? TokenExpiresAt { get; set; }
}

