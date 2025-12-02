namespace backend.DTOs.User.Response;

/// <summary>
/// User response DTO (without ID for security)
/// </summary>
public class UserResponse
{
    public string Email { get; init; } = string.Empty;
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Bio { get; init; }
    public string? Location { get; init; }
    public string? AvatarUrl { get; init; }
    public bool IsActive { get; init; }
    public bool IsAdmin { get; init; }
    public DateTime? LastLoginAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

