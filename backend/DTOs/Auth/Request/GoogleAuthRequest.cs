using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using backend.Common.Enums;

namespace backend.DTOs.Auth.Request;

/// <summary>
/// Request DTO for Google OAuth authentication
/// </summary>
public class GoogleAuthRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    public string? FirstName { get; init; }

    public string? LastName { get; init; }

    public string? AvatarUrl { get; init; }

    [Required]
    [JsonRequired]
    public OAuthProviderType Provider { get; init; }

    [Required]
    public string ProviderId { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    public string ProviderEmail { get; init; } = string.Empty;

    [Required]
    public string AccessToken { get; init; } = string.Empty;

    public string? RefreshToken { get; init; }

    public DateTime? TokenExpiresAt { get; init; }
}

