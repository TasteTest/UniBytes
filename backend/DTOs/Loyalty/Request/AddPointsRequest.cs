using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace backend.DTOs.Loyalty.Request;

/// <summary>
/// Request DTO for adding points to an account
/// </summary>
public class AddPointsRequest
{
    [Required]
    [JsonRequired]
    public Guid UserId { get; init; }

    [Required]
    [Range(1, long.MaxValue, ErrorMessage = "Points must be greater than 0")]
    public long Points { get; init; }

    [Required]
    [MaxLength(255)]
    public string Reason { get; init; } = string.Empty;

    public Guid? ReferenceId { get; init; }

    public string? Metadata { get; set; }
}
