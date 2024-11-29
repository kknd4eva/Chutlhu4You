using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using FargateAPIApp.Features.Orders.Models;
using Amazon.DynamoDBv2.DocumentModel;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace FargateAPIApp.Shared.Repositories.Orders;

public class OrderRepository : IOrderRepository<Order>
{
    private readonly IAmazonDynamoDB _dynamoDbClient; // for low level operations like table management.
    private readonly IDynamoDBContext _dynamoDbContext; // for high level operations like CRUD.

    public OrderRepository(IAmazonDynamoDB dynamoDbClient, IDynamoDBContext dynamoDbContext)
        => (_dynamoDbClient, _dynamoDbContext)
        = (dynamoDbClient, dynamoDbContext);

    public async Task CreateOrderAsync(Order order)
    {
        await _dynamoDbContext.SaveAsync(order);
    }

    public Task DeleteOrderAsync(string id)
    {
        throw new NotImplementedException();
    }

    public async Task<Order> GetOrderAsync(string customerId, string orderId)
    {
        // get order by customer id and the first part of the sort key using a query.
        var query = _dynamoDbContext
            .QueryAsync<Order>(customerId, QueryOperator.BeginsWith, new List<object> { orderId });

        var orders = await query.GetRemainingAsync();

        return orders.FirstOrDefault();
    }

    public Task<IEnumerable<Order>> GetOrdersAsync()
    {
        throw new NotImplementedException();
    }

    public Task UpdateOrderAsync(string id, Order order)
    {
        throw new NotImplementedException();
    }
}
