using backend.Common;
using backend.Common.Enums;

namespace backend.Models;

/// <summary>
/// User model
/// </summary>
public class User : BaseEntity
{
    public string Email { get; init; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Bio { get; init; }
    public string? Location { get; init; }
    public string? AvatarUrl { get; set; }
    public bool IsActive { get; init; } = true;
    public UserRole Role { get; set; } = UserRole.User;
    public DateTime? LastLoginAt { get; set; }

    // Navigation properties
    public virtual ICollection<OAuthProvider> OAuthProviders { get; init; } = new List<OAuthProvider>();
    public virtual ICollection<UserAnalytics> UserAnalytics { get; init; } = new List<UserAnalytics>();
}

