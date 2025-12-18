using System.Text.Json;
using System.Text.Json.Serialization;

namespace backend.DTOs.Menu.Response;

/// <summary>
/// Menu item response DTO
/// </summary>
public class MenuItemResponseDto
{
    public Guid Id { get; init; }
    public Guid CategoryId { get; init; }
    
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public string Currency { get; init; } = "ron";
    public bool Available { get; init; }
    public int Visibility { get; init; }
    public JsonDocument? Components { get; init; }
    public string? ImageUrl { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}