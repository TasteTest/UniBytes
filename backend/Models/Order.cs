using System.Text.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

public class Order
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string? ExternalUserRef { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public int PaymentStatus { get; set; }
    public int OrderStatus { get; set; }
    public DateTime PlacedAt { get; init; }
    public DateTime? CancelRequestedAt { get; set; }
    public DateTime? CanceledAt { get; set; }
    [Column(TypeName = "jsonb")]
    public JsonDocument? Metadata { get; set; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}