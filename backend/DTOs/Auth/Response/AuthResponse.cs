using System.Text.Json.Serialization;
using backend.DTOs.User.Response;

namespace backend.DTOs.Auth.Response;

/// <summary>
/// Response DTO for authentication
/// </summary>
public class AuthResponse
{
    public string UserId { get; init; } = string.Empty;
    public UserResponse? User { get; init; } = null!;
    public bool IsNewUser { get; init; }
    public string Message { get; init; } = string.Empty;
}
