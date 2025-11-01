using backend_user.Common;

namespace backend_user.Model;

/// <summary>
/// User model matching the database schema
/// </summary>
public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Bio { get; set; }
    public string? Location { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsAdmin { get; set; } = false;
    public DateTime? LastLoginAt { get; set; }

    // Navigation properties
    public virtual ICollection<OAuthProvider> OAuthProviders { get; set; } = new List<OAuthProvider>();
    public virtual ICollection<UserAnalytics> UserAnalytics { get; set; } = new List<UserAnalytics>();
}

