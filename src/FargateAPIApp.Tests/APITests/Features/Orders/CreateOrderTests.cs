using AWS.Messaging;
using FargateAPIApp.Features.Orders.DTOs;
using FargateAPIApp.Features.Orders;
using FargateAPIApp.Shared.Repositories.Orders;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FargateAPIApp.Features.Orders.Models;
using FluentValidation.Results;
using AWS.Messaging.Publishers;
using FargateAPIApp.Shared.Models;

namespace FargateAPIApp.Tests.APITests.Features.Orders
{
    public class CreateOrderTests
    {
        private readonly ISender _sender;
        private readonly IValidator<CreateOrderCommand> _validator;
        private readonly IMessagePublisher _messagePublisher;
        private readonly IOrderRepository<Order> _orderRepository;
        private readonly ILogger<CreateOrderCommandHandler> _logger;
        private readonly OrderMapper _orderMapper;

        public CreateOrderTests()
        {
            _sender = Substitute.For<ISender>();
            _validator = Substitute.For<IValidator<CreateOrderCommand>>();
            _messagePublisher = Substitute.For<IMessagePublisher>();
            _orderRepository = Substitute.For<IOrderRepository<Order>>();
            _logger = Substitute.For<ILogger<CreateOrderCommandHandler>>();
            _orderMapper = new OrderMapper();
        }

        [Fact]
        public async Task CreateOrderEndpoint_ShouldReturnBadRequest_WhenValidationFails()
        {
            // Arrange
            var orderDto = new OrderDto
            {
                OrderId = string.Empty, // Invalid OrderId
                OrderItems = new List<FargateAPIApp.Features.Orders.DTOs.OrderItem>() 
            };
            var command = new CreateOrderCommand(orderDto);

            _validator.ValidateAsync(command)
                      .Returns(new ValidationResult(new[]
                      {
                          new ValidationFailure("OrderId", "OrderId must not be null or empty")
                      }));

            // Act
            var validationResult = await _validator.ValidateAsync(command);

            // Assert
            Assert.False(validationResult.IsValid);
            Assert.Contains(validationResult.Errors, e => e.PropertyName == "OrderId");
        }

        [Fact]
        public async Task CreateOrderCommandHandler_ShouldReturnResponse_WhenOrderCreatedSuccessfully()
        {
            // Arrange
            var orderDto = new OrderDto
            {
                OrderId = "123",
                CustomerId = "C456",
                OrderDate = DateTime.UtcNow,
                OrderItems = new List<FargateAPIApp.Features.Orders.DTOs.OrderItem>() 
            };
            var command = new CreateOrderCommand(orderDto);
            var publishResponse = Substitute.For<IPublishResponse>();
            publishResponse.MessageId.Returns("message-id");

            _orderRepository.CreateOrderAsync(Arg.Any<Order>()).Returns(Task.CompletedTask);
            _messagePublisher.PublishAsync(command).Returns(publishResponse);

            var handler = new CreateOrderCommandHandler(
                _messagePublisher, 
                _logger, 
                _orderRepository, 
                _orderMapper);

            // Act
            var response = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(response);
            Assert.Equal("message-id", response.MessageId);
            Assert.Equal(orderDto, response.Order);
        }

        [Fact]
        public async Task CreateOrderCommandHandler_ShouldThrowException_WhenPublishingFails()
        {
            // Arrange
            var orderDto = new OrderDto
            {
                OrderId = "123",
                CustomerId = "C456",
                OrderDate = DateTime.UtcNow,
                OrderItems = new List<FargateAPIApp.Features.Orders.DTOs.OrderItem>() // Initialize OrderItems
            };
            var command = new CreateOrderCommand(orderDto);

            _orderRepository.CreateOrderAsync(Arg.Any<Order>()).Returns(Task.CompletedTask);
            _messagePublisher.PublishAsync(command).Returns(Task.FromException<IPublishResponse>(new FailedToPublishException("Failed to publish message")));

            var handler = new CreateOrderCommandHandler(
                _messagePublisher, 
                _logger, 
                _orderRepository,
                _orderMapper);

            // Act & Assert
            await Assert.ThrowsAsync<FailedToPublishException>(() => handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task CreateOrderCommandHandler_ShouldThrowException_WhenSavingToRepositoryFails()
        {
            // Arrange
            var orderDto = new OrderDto
            {
                OrderId = "123",
                CustomerId = "C456",
                OrderDate = DateTime.UtcNow,
                OrderItems = new List<FargateAPIApp.Features.Orders.DTOs.OrderItem>() // Initialize OrderItems
            };
            var command = new CreateOrderCommand(orderDto);

            _orderRepository.CreateOrderAsync(Arg.Any<Order>()).Returns(Task.FromException(new Exception("Repository failure")));

            var handler = new CreateOrderCommandHandler(
                _messagePublisher, 
                _logger, 
                _orderRepository,
                _orderMapper);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => handler.Handle(command, CancellationToken.None));
        }
    }
}
