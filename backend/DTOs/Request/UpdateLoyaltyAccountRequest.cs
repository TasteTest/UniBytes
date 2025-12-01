namespace backend.DTOs.Request;

/// <summary>
/// Request DTO for updating a loyalty account
/// </summary>
public class UpdateLoyaltyAccountRequest
{
    public long? PointsBalance { get; set; }

    public int? Tier { get; set; }

    public bool? IsActive { get; set; }
}
