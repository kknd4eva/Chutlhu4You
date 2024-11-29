using FargateAPIApp.Features.Orders;
using FargateAPIApp.Features.Orders.DTOs;
using FargateAPIApp.Features.Orders.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using FargateAPIApp.Shared.Repositories.Orders;
using OrderItem = FargateAPIApp.Features.Orders.Models.OrderItem;

namespace Fargate_APIApp.Tests.APITests.Features.Orders
{
    public class OrderMappingTests
    {
        [Fact]
        public async Task OrderDto_WhenConvertedToModel_Succeeds()
        {
            // Arrange
            var orderDto = new OrderDto
            {
                OrderId = "123",
                CustomerId = "0000",
                OrderDate = DateTime.UtcNow, 
                OrderItems = new List<FargateAPIApp.Features.Orders.DTOs.OrderItem>
                {
                    new FargateAPIApp.Features.Orders.DTOs.OrderItem(
                        "4298",
                        "Test Product 1",
                        "Test Description 1",
                        5,
                        1,
                        3
                    )
                }
                
            };

            // Act
            var mapper = new OrderMapper();
            var result = mapper.TryMapToModel(orderDto, out var order);

            // Assert
            Assert.NotNull(order);
            Assert.True(result);
            Assert.Equal("123", order.GetOrderId());
            Assert.Equal("0000", order.Id);
            Assert.Equal(1, order.OrderItems.Count);
            Assert.Equal("4298", order.OrderItems[0].ProductId);
            Assert.Equal("Test Product 1", order.OrderItems[0].ProductName);
            Assert.Equal("Test Description 1", order.OrderItems[0].Description);
            Assert.Equal(1, order.OrderItems[0].Price);
            Assert.Equal(3, order.OrderItems[0].Discount);
            Assert.Equal(5, order.OrderItems[0].Quantity);
        }

        [Fact]
        public async Task OrderModel_WhenConvertedToDto_Succeeds()
        {
            // Arrange
            var order = new Order("123", "0000", DateTime.UtcNow,
                new List<FargateAPIApp.Features.Orders.Models.OrderItem>
            {
                new FargateAPIApp.Features.Orders.Models.OrderItem
                {
                    ProductId = "4298",
                    ProductName = "Test Product 1",
                    Description = "Test Description 1",
                    Price = 5,
                    Discount = 0,
                    Quantity = 3
                }
            });

            // Act
            var mapper = new OrderMapper();
            var result = mapper.TryMapToDto(order, out var dto);
            
            Assert.NotNull(dto);
            Assert.True(result);
            Assert.Equal("123", dto.OrderId);
            Assert.Equal("0000", dto.CustomerId);
            Assert.Equal(1, dto.OrderItems.Count);
            Assert.Equal("4298", dto.OrderItems[0].Sku);
            Assert.Equal("Test Product 1", dto.OrderItems[0].ProductName);
            Assert.Equal("Test Description 1", dto.OrderItems[0].ProductDescription);
            Assert.Equal(5, dto.OrderItems[0].Price);
            Assert.Equal(0, dto.OrderItems[0].Discount);
            Assert.Equal(3, dto.OrderItems[0].Quantity);

        }
    }
}
