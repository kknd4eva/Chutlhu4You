using FargateAPIApp.Shared.Models;

namespace FargateAPIApp.Shared.Repositories.Orders
{
    public interface IOrderRepository<TModel> where TModel : IModel
    {
        Task<IEnumerable<TModel>> GetOrdersAsync();
        Task<TModel> GetOrderAsync(string customerId, string orderId);
        Task CreateOrderAsync(TModel order);
        Task UpdateOrderAsync(string id, TModel order);
        Task DeleteOrderAsync(string id);
    }
}
