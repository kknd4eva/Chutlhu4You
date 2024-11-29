using Amazon.DynamoDBv2.DataModel;

namespace FargateAPIApp.Features.Orders.Models
{
    public class OrderItem
    {
        [DynamoDBProperty("product_id")]
        public string ProductId { get; set; }
        [DynamoDBProperty("quantity")]
        public int Quantity { get; set; }
        [DynamoDBProperty("product_name")]
        public string ProductName { get; set; }
        [DynamoDBProperty("description")]
        public string Description { get; set; }
        [DynamoDBProperty("price")]
        public decimal Price { get; set; }
        [DynamoDBProperty("discount")]
        public decimal Discount { get; set; }

    }
}
