using System.Text.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

public class OrderItem
{
    public Guid Id { get; init; }
    public Guid OrderId { get; init; }
    public Guid MenuItemId { get; init; }
    public string Name { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    [Column(TypeName = "jsonb")]
    public JsonDocument? Modifiers { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime CreatedAt { get; init; }
    
    // Navigation property
    public virtual Order? Order { get; set; }
}