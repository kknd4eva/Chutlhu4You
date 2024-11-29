namespace FargateAPIApp.Features.Orders.DTOs
{
    public class OrderDto
    {
        public required string OrderId { get; set; }
        public string? CustomerId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public required List<OrderItem> OrderItems { get; set; }
    }
}
