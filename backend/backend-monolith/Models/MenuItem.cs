using System.Text.Json;

namespace backend_monolith.Modelss;

/// <summary>
/// Menu item model
/// </summary>
public class MenuItem
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "RON";
    public bool Available { get; set; } = true;
    public int Visibility { get; set; } = 1;
    public JsonDocument? Components { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public MenuCategory Category { get; set; } = default!;
}

