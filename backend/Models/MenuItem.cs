using System.Text.Json;

namespace backend.Models;

/// <summary>
/// Menu item model
/// </summary>
public class MenuItem
{
    public Guid Id { get; init; }
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "RON";
    public bool Available { get; set; } = true;
    public int Visibility { get; init; } = 1;
    public JsonDocument? Components { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public MenuCategory Category { get; init; } = null!;
}

