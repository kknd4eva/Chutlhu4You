using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2;
using FargateAPIApp.Features.Orders;
using FargateAPIApp.Features.Orders.Models;
using FargateAPIApp.Shared.Repositories.Orders;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FargateAPIApp.Tests.APITests.Features.Orders
{
    public class GetOrderTests
    {
        [Fact]
        public async Task GetOrderAsync_ReturnsOrder_WhenOrderExists()
        {
            // Arrange
            var mockDynamoDbContext = Substitute.For<IDynamoDBContext>();
            var mockDynamoDbClient = Substitute.For<IAmazonDynamoDB>();

            var customerId = "customer123";
            var orderId = "order456";
            var expectedOrder = new Order(orderId, customerId, DateTime.Now, new List<OrderItem> { new OrderItem { ProductName = "" } });

            var mockAsyncSearch = Substitute.For<AsyncSearch<Order>>();
            mockAsyncSearch.GetRemainingAsync(CancellationToken.None).Returns(new List<Order> { expectedOrder });

            mockDynamoDbContext
                .QueryAsync<Order>(customerId, QueryOperator.BeginsWith, Arg.Is<List<object>>(list => list.Contains(orderId)), null)
                .Returns(mockAsyncSearch);

            var repository = new OrderRepository(mockDynamoDbClient, mockDynamoDbContext);

            // Act
            var result = await repository.GetOrderAsync(customerId, orderId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedOrder.Id, result.Id);
        }

        [Fact]
        public async Task GetOrderAsync_ReturnsNull_WhenOrderDoesNotExist()
        {
            // Arrange
            var mockDynamoDbContext = Substitute.For<IDynamoDBContext>();
            var mockDynamoDbClient = Substitute.For<IAmazonDynamoDB>();

            var customerId = "customer123";
            var orderId = "order456";

            var mockAsyncSearch = Substitute.For<AsyncSearch<Order>>();
            mockAsyncSearch.GetRemainingAsync(CancellationToken.None).Returns(new List<Order>());

            mockDynamoDbContext
                .QueryAsync<Order>(customerId, QueryOperator.BeginsWith, Arg.Is<List<object>>(list => list.Contains(orderId)), null)
                .Returns(mockAsyncSearch);

            var repository = new OrderRepository(mockDynamoDbClient, mockDynamoDbContext);

            // Act
            var result = await repository.GetOrderAsync(customerId, orderId);

            // Assert
            Assert.Null(result);

        }
    }
}
