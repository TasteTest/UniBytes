namespace backend_monolith.Modelss;

/// <summary>
/// Menu category model
/// </summary>
public class MenuCategory
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
}

