namespace FargateAPIApp.Features.Orders.DTOs
{
    public record OrderItem(string Sku, string ProductName, string ProductDescription, int Quantity, decimal Price, decimal Discount);
}
