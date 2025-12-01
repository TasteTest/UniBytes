using System.ComponentModel.DataAnnotations;

namespace backend_monolith.DTOs.Request;

/// <summary>
/// Request DTO for creating a new user
/// </summary>
public class CreateUserRequest
{
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? FirstName { get; set; }

    [MaxLength(100)]
    public string? LastName { get; set; }

    public string? Bio { get; set; }

    [MaxLength(100)]
    public string? Location { get; set; }

    public string? AvatarUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsAdmin { get; set; } = false;
}

