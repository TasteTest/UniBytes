using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.UserAnalytics.Request;

/// <summary>
/// Request DTO for creating user analytics event
/// </summary>
public class CreateUserAnalyticsRequest
{
    [Required]
    public Guid UserId { get; init; }

    [Required]
    [MaxLength(255)]
    public string SessionId { get; init; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string EventType { get; init; } = string.Empty;

    public string EventData { get; set; } = "{}";

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public string? ReferrerUrl { get; set; }
}

