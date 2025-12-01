using System.Net;

namespace backend_monolith.Modelss;

/// <summary>
/// User analytics model for tracking user events
/// </summary>
public class UserAnalytics
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string EventData { get; set; } = "{}";
    public IPAddress? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? ReferrerUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public virtual User User { get; set; } = null!;
}

