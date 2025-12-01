using System.ComponentModel.DataAnnotations;

namespace backend_monolith.DTOs.Request;

/// <summary>
/// Request DTO for adding points to an account
/// </summary>
public class AddPointsRequest
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    [Range(1, long.MaxValue, ErrorMessage = "Points must be greater than 0")]
    public long Points { get; set; }

    [Required]
    [MaxLength(255)]
    public string Reason { get; set; } = string.Empty;

    public Guid? ReferenceId { get; set; }

    public string? Metadata { get; set; }
}
