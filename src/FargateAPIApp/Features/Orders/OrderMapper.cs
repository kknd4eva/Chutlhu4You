using FargateAPIApp.Features.Orders.DTOs;
using FargateAPIApp.Features.Orders.Models;
using FargateAPIApp.Shared.Models;

namespace FargateAPIApp.Features.Orders
{
    public class OrderMapper : IMapper<Order, OrderDto>
    {
        /// <summary>
        /// Maps an Order model to an OrderDto
        /// </summary>
        /// <param name="model"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        public bool TryMapToDto(Order model, out OrderDto dto)
        {
            (string orderId, DateTime orderDate) = model.SplitSortKey();

            try
            {
                dto = new OrderDto
                {
                    OrderId = orderId,
                    CustomerId = model.Id,
                    OrderDate = orderDate,
                    OrderItems = model.OrderItems.Select(
                        i => new DTOs.OrderItem(
                        i.ProductId,
                        i.ProductName,
                        i.Description,
                        i.Quantity,
                        i.Price,
                        i.Discount)).ToList()
                };
            }
            catch (Exception)
            {
                dto = null;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Maps an OrderDto to an Order model
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool TryMapToModel(OrderDto dto, out Order model)
        {
            try
            {
                model = new Order(
                    dto.OrderId,
                    dto.CustomerId,
                    dto.OrderDate,
                    dto.OrderItems.Select(
                        i => new Models.OrderItem
                        {
                            ProductId = i.Sku,
                            ProductName = i.ProductName,
                            Description = i.ProductDescription,
                            Quantity = i.Quantity,
                            Price = i.Price,
                            Discount = i.Discount
                        }).ToList());
            }
            catch (Exception)
            {
                model = null;
                return false;
            }

            return true;
        }
    }
}
