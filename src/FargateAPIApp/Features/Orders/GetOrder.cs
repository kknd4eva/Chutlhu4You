using FargateAPIApp.Features.Orders.DTOs;
using FargateAPIApp.Features.Orders.Models;
using FargateAPIApp.Shared.Models;
using FargateAPIApp.Shared.Repositories.Orders;
using MediatR;

namespace FargateAPIApp.Features.Orders;

public record GetOrderResponse(OrderDto? Order);
public record GetOrderQuery(string customerId, string orderId) : IRequest<GetOrderResponse>;

public static class GetOrderEndpoint
{
    public static void MapGetOrderEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/{customer}/{id}", async (ISender sender, string customer, string id) =>
        {
            var query = new GetOrderQuery(customerId: customer, orderId: id);

            var result = await sender.Send(query);

            return result.Order == null
                ? Results.NotFound()
                : Results.Ok(result.Order);

        })
        .WithDescription("Get an order by its ID")
        .WithName("GetOrder")
        .WithSummary("Get an order by its ID");
    }
}

internal sealed class GetOrderQueryHandler : IRequestHandler<GetOrderQuery, GetOrderResponse>
{
    private readonly IOrderRepository<Order> _orderRepository;
    private readonly ILogger<GetOrderQueryHandler> _logger;
    private readonly IMapper<Order, OrderDto> _orderMapper;

    public GetOrderQueryHandler(IOrderRepository<Order> orderRepository, 
        ILogger<GetOrderQueryHandler> logger, 
        IMapper<Order, OrderDto> orderMapper)
        => (_orderRepository, _logger, _orderMapper)
        = (orderRepository, logger, orderMapper);

    public async Task<GetOrderResponse> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting order with ID {OrderId}", request.orderId); 

        var order = await _orderRepository.GetOrderAsync(request.customerId,request.orderId);

        if (order == null)
        {
            _logger.LogWarning("Order with ID {OrderId} not found", request.orderId);
            return new GetOrderResponse(null);
        }
        else {
            _orderMapper.TryMapToDto(order, out var orderDto);
            return new GetOrderResponse(orderDto);
        }

    }
}
