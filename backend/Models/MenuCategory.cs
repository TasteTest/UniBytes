namespace backend.Models;

/// <summary>
/// Menu category model
/// </summary>
public class MenuCategory
{
    public Guid Id { get; init; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public ICollection<MenuItem> MenuItems { get; init; } = new List<MenuItem>();
}

