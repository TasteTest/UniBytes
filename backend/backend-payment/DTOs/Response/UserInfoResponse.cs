namespace backend_payment.DTOs.Response;

/// <summary>
/// User information from backend-user service
/// </summary>
public class UserInfoResponse
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? AvatarUrl { get; set; }
}

