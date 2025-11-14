namespace DefaultNamespace;
public class OrderItem
{
    public Guid id { get; set; }
    public Guid order_id { get; set; }
    public Guid menu_item_id { get; set; }
    public string name { get; set; }
    public decimal unit_price { get; set; }
    public int quantity { get; set; }
    public string modifiers { get; set; }
    public decimal total_price { get; set; }
    public DateTime created_at { get; set; }
}