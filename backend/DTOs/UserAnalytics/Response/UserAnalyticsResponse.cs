namespace backend.DTOs.UserAnalytics.Response;

/// <summary>
/// User analytics response DTO (without ID for security)
/// </summary>
public class UserAnalyticsResponse
{
    public string? SessionId { get; init; } = string.Empty;
    public string? EventType { get; init; } = string.Empty;
    public string? EventData { get; init; } = "{}";
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    public string? ReferrerUrl { get; init; }
    public DateTime CreatedAt { get; init; }
}

