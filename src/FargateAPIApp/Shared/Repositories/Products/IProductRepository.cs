using FargateAPIApp.Shared.Models;

namespace FargateAPIApp.Shared.Repositories.Products
{
    public interface IProductRepository<TModel> where TModel : IModel
    {
        Task<IEnumerable<TModel>> GetProductsAsync();
        Task<TModel> GetProductAsync(string id);
        Task CreateProductAsync(TModel product);
        Task UpdateProductAsync(string id, TModel product);
        Task DeleteProductAsync(string id);
    }
}
