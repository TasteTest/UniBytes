using System.Text.Json.Serialization;

namespace backend.DTOs.User.Response;

/// <summary>
/// User information from backend-user service
/// </summary>
public class UserInfoResponse
{
    /// <summary>Internal use only - not serialized to JSON</summary>
    [JsonIgnore]
    public string Id { get; set; } = string.Empty;
    
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? AvatarUrl { get; set; }
}

