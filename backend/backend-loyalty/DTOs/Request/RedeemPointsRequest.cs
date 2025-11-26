using System.ComponentModel.DataAnnotations;

namespace backend_loyalty.DTOs.Request;

/// <summary>
/// Request DTO for redeeming points
/// </summary>
public class RedeemPointsRequest
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    [Range(1, long.MaxValue, ErrorMessage = "Points must be greater than 0")]
    public long Points { get; set; }

    [Required]
    [MaxLength(100)]
    public string RewardType { get; set; } = string.Empty;

    public string? RewardMetadata { get; set; }
}
