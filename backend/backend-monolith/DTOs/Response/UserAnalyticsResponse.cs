namespace backend_monolith.DTOs.Response;

/// <summary>
/// User analytics response DTO (without ID for security)
/// </summary>
public class UserAnalyticsResponse
{
    public string SessionId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string EventData { get; set; } = "{}";
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? ReferrerUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}

