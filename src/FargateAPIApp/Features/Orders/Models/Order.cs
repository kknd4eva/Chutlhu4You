using Amazon.DynamoDBv2.DataModel;
using FargateAPIApp.Shared.Models;

namespace FargateAPIApp.Features.Orders.Models
{
    [DynamoDBTable("webshop_table")]
    public class Order(
                string OrderId,
                string? CustomerId,
                DateTime OrderDate,
                List<OrderItem> OrderItems) : IModel
    {
        [DynamoDBHashKey("id")]
        public string Id { get; set; } = CustomerId;
        [DynamoDBRangeKey("sort")]
        public string Sort { get; set; } = $"{OrderId}#{OrderDate}";
        [DynamoDBProperty("order_date")]
        public DateTime OrderDate { get; set; } = OrderDate;
        [DynamoDBProperty("order_items")]
        public List<OrderItem> OrderItems { get; set; } = OrderItems;

        public Order() : this(default!, default, default, new List<OrderItem>()) { }

        public (string orderId, DateTime orderDate) SplitSortKey()
        {
            var split = Sort.Split('#');
            return (split[0],
                DateTime.Parse(split[1]));
        }

        public string GetOrderId()
        {
            return SplitSortKey().orderId;
        }

        public DateTime GetOrderDate()
        {
            return SplitSortKey().orderDate;
        }
    }
}
