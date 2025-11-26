using System.ComponentModel.DataAnnotations;

namespace backend_loyalty.DTOs.Request;

/// <summary>
/// Request DTO for creating a new loyalty account
/// </summary>
public class CreateLoyaltyAccountRequest
{
    [Required]
    public Guid UserId { get; set; }

    public long PointsBalance { get; set; } = 0;

    public int Tier { get; set; } = 0; // Bronze by default

    public bool IsActive { get; set; } = true;
}
