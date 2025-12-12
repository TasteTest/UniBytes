using System.Text.Json.Serialization;

namespace backend.DTOs.Menu.Response;

/// <summary>
/// Category response DTO
/// </summary>
public class CategoryResponseDto
{
    /// <summary>Internal use only - not serialized to JSON</summary>
    [JsonIgnore]
    public Guid Id { get; init; }
    
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int DisplayOrder { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}