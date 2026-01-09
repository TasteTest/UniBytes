using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace backend.DTOs.Loyalty.Request;

/// <summary>
/// Request DTO for creating a new loyalty account
/// </summary>
public class CreateLoyaltyAccountRequest
{
    [Required]
    [JsonRequired]
    public Guid UserId { get; init; }

    public long PointsBalance { get; set; } = 0;

    public int Tier { get; set; } = 0; // bronze by default

    public bool IsActive { get; set; } = true;
}
