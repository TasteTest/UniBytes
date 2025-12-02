using System.Net;

namespace backend.Models;

/// <summary>
/// User analytics model for tracking user events
/// </summary>
public class UserAnalytics
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid UserId { get; init; }
    public string SessionId { get; init; } = string.Empty;
    public string EventType { get; init; } = string.Empty;
    public string EventData { get; init; } = "{}";
    public IPAddress? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    public string? ReferrerUrl { get; init; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public virtual User User { get; init; } = null!;
}

