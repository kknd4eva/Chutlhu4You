namespace FargateOrderProcessor.Entities 
{
    public record CreateOrderCommand(Order Order);
    public class Order
    {
        public required string OrderId { get; set; }
        public string? CustomerId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public required List<OrderItem> OrderItems { get; set; }
    }
}
