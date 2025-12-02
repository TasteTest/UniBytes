using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.User.Request;

/// <summary>
/// Request DTO for updating an existing user
/// </summary>
public class UpdateUserRequest
{
    [EmailAddress]
    [MaxLength(255)]
    public string? Email { get; init; }

    [MaxLength(100)]
    public string? FirstName { get; init; }

    [MaxLength(100)]
    public string? LastName { get; init; }

    public string? Bio { get; set; }

    [MaxLength(100)]
    public string? Location { get; set; }

    public string? AvatarUrl { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsAdmin { get; set; }
}

