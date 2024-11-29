using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using FargateAPIApp.Features.Products.Models;

namespace FargateAPIApp.Shared.Repositories.Products;

    public class ProductRepository : IProductRepository<Product>
    {
        private readonly IAmazonDynamoDB _dynamoDbClient; // for low level operations like table management.
        private readonly IDynamoDBContext _dynamoDbContext; // for high level operations like CRUD.

        public ProductRepository(IAmazonDynamoDB dynamoDbClient, IDynamoDBContext dynamoDbContext) 
            => (_dynamoDbClient, _dynamoDbContext) 
            = (dynamoDbClient, dynamoDbContext);

        public async Task CreateProductAsync(Product product)
        {
           await _dynamoDbContext.SaveAsync(product);
        }

        public async Task DeleteProductAsync(string id)
        {
            await _dynamoDbContext.DeleteAsync<Product>(id);
        }

        public async Task<Product> GetProductAsync(string id)
        {
            return await _dynamoDbContext.LoadAsync<Product>(id);
        }

        public async Task<IEnumerable<Product>> GetProductsAsync()
        {
            return await _dynamoDbContext.ScanAsync<Product>(new List<ScanCondition>()).GetRemainingAsync();
        }

        public async Task UpdateProductAsync(string id, Product product)
        {
            var updatedProduct = await _dynamoDbContext.LoadAsync<Product>(id);

            if (updatedProduct == null)
            {
                throw new KeyNotFoundException();
            }

            await _dynamoDbContext.SaveAsync(updatedProduct);
        }
    }

