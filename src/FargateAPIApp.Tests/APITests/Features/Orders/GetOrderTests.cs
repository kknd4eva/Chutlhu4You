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
        public async Task GetOrderEndpoint_ShouldReturnNotFound_WhenOrderDoesNotExist()
        {
            var orderRepository = Substitute.For<IOrderRepository<Order>>();
            var logger = Substitute.For<ILogger<GetOrderQueryHandler>>();
            var orderMapper = new OrderMapper();

            var handler = new GetOrderQueryHandler(orderRepository, logger, orderMapper);

            var o = orderRepository.GetOrderAsync("customer1", "order1").Returns(Task.FromResult<Order?>(null));
            var result = await handler.Handle(new GetOrderQuery("customer1", "order1"), default);

            Assert.Null(result.Order);



        }
    }
}
