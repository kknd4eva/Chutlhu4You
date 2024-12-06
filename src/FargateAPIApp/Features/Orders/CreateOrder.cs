using AWS.Messaging;
using FargateAPIApp.Features.Orders.DTOs;
using FargateAPIApp.Features.Orders.Models;
using FargateAPIApp.Shared.Models;
using FargateAPIApp.Shared.Repositories.Orders;
using FluentValidation;
using MediatR;
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("FargateAPIApp.Unit.Tests")]

namespace FargateAPIApp.Features.Orders;
public record CreateOrderResponse(OrderDto Order, string MessageId);
public record CreateOrderCommand(OrderDto Order) : IRequest<CreateOrderResponse>;
public static class CreateOrderEndpoint
{
    public static void MapCreateOrderEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/order", async (OrderDto order,
                                     ISender sender,
                                     IValidator<CreateOrderCommand> validator) =>
        {
            try
            {
                // Create the command
                var command = new CreateOrderCommand(order);

                var validationResult = await validator.ValidateAsync(command);

                if (!validationResult.IsValid)
                {
                    // Return a 400 Bad Request with validation errors
                    var errors = validationResult.Errors
                    .Select(x => new { x.PropertyName, x.ErrorMessage });
                    
                    return Results.BadRequest(new { Errors = errors });
                }

                var result = await sender.Send(command); 
                return Results.Created("/order", result);
            }
            catch (Exception ex)
            {
                // Handle unexpected errors
                return Results.BadRequest(ex.Message);
            }

        })
        .WithDescription("Create a new sales order")
        .Accepts<OrderDto>("application/json")
        .WithName("CreateOrder")
        .WithSummary("Create a new sales order");
    }
}

internal sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    /// <summary>
    /// This validator ensures that components required for our DynamoDb schema are present.
    /// </summary>
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.Order.OrderId).NotNull().NotEmpty().WithMessage("OrderId must not be null or empty");
        RuleFor(x => x.Order.CustomerId).NotNull().NotEmpty().WithMessage("CustomerId must not be null or empty");
        RuleFor(x => x.Order.OrderDate).NotNull().NotEmpty().WithMessage("OrderDate must not be null or empty");
        RuleFor(x => x.Order.OrderItems).NotEmpty().WithMessage("Order must have at least one item");
    }
}

internal sealed class  CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, CreateOrderResponse>
{
    private readonly IMessagePublisher _messagePublisher;
    private readonly IOrderRepository<Order> _orderRepository;
    private readonly ILogger<CreateOrderCommandHandler> _logger;
    private readonly IMapper<Order,OrderDto> _orderMapper;

    public CreateOrderCommandHandler(IMessagePublisher messagePublisher, 
        ILogger<CreateOrderCommandHandler> logger,
        IOrderRepository<Order> orderRepository,
        IMapper<Order, OrderDto> orderMapper) =>
        (_messagePublisher, _logger, _orderRepository, _orderMapper) = 
        (messagePublisher, logger, orderRepository, orderMapper);

    public async Task<CreateOrderResponse> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Saving order {OrderId} to repository.", request.Order.OrderId);    

            _orderMapper.TryMapToModel(request.Order, out var orderModel);

            await _orderRepository.CreateOrderAsync(orderModel);

            _logger.LogInformation("Publishing to target queue:" + Environment.GetEnvironmentVariable("SQS_PUBLISH_QUEUE"));
            var publishResponse = await _messagePublisher.PublishAsync(request);

            _logger.LogInformation("Message published to queue, messageId:: {PublishResponse}", publishResponse.MessageId);

            return new CreateOrderResponse(request.Order, publishResponse.MessageId);
        }
        catch (FailedToPublishException publishEx)
        {
            _logger.LogError(publishEx, "Failed to publish message to queue");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save order {OrderId} to repository", request.Order.OrderId);
            throw;
        }
    }
}
