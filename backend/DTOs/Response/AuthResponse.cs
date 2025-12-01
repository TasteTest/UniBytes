namespace backend.DTOs.Response;

/// <summary>
/// Response DTO for authentication
/// </summary>
public class AuthResponse
{
    public string UserId { get; set; } = string.Empty;
    public UserResponse User { get; set; } = null!;
    public bool IsNewUser { get; set; }
    public string Message { get; set; } = string.Empty;
}

