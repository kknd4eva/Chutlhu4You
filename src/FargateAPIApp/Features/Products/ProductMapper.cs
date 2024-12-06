using FargateAPIApp.Features.Products.Models;
using FargateAPIApp.Shared.Models;
using FargateWeb.DTOs.ProductDto;

namespace FargateAPIApp.Features.Products
{
    public class ProductMapper : IMapper<Product, ProductDto>
    {
        public bool TryMapToDto(Product model, out ProductDto dto)
        {
            try
            {
                dto = new ProductDto
                {
                    Sku = model.Id,
                    Name = model.Name,
                    Description = model.Description,
                    Price = model.Price,
                    Category = model.Range,
                    DiscountPrice = model.DiscountPrice,
                    ImageUrl = model.ImageUrl,
                    ThumbnailUrl = model.ThumbnailUrl
                };
            }
            catch (Exception)
            {
                dto = null;
                return false;
            }

            return true;
        }

        public bool TryMapToModel(ProductDto dto, out Product model)
        {
            try
            {
                model = new Product(
                    dto.Name,
                    dto.Description,
                    dto.Category,
                    dto.Price,
                    dto.DiscountPrice,
                    dto.ImageUrl,
                    dto.ThumbnailUrl,
                    dto.Sku
                );
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
