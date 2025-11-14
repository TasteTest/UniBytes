namespace DefaultNamespace;
public class Order
{
    public Guid id { get; set; }
    public Guid user_id { get; set; }
    public string external_user_ref { get; set; }
    public decimal total_amount { get; set; }
    public string currency { get; set; }
    public int payment_status { get; set; }
    public int order_status { get; set; }
    public DateTime cancel_requested_at { get; set; }
    public DateTime canceled_at { get; set; }
    public string metadata { get; set; }
}