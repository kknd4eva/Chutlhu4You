namespace FargateOrderProcessor.Entities
{
    public record OrderItem(int sku, string productName, int quantity, decimal price, decimal discount);
}
