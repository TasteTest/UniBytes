using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Loyalty.Request;

/// <summary>
/// Request DTO for redeeming points
/// </summary>
public class RedeemPointsRequest
{
    [Required]
    public Guid UserId { get; init; }

    [Required]
    [Range(1, long.MaxValue, ErrorMessage = "Points must be greater than 0")]
    public long Points { get; init; }

    [Required]
    [MaxLength(100)]
    public string RewardType { get; init; } = string.Empty;

    public string? RewardMetadata { get; init; }
}
