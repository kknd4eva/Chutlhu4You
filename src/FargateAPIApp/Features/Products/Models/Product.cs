using Amazon.DynamoDBv2.DataModel;
using FargateAPIApp.Shared.Models;
using System.ComponentModel.DataAnnotations;

namespace FargateAPIApp.Features.Products.Models
{
    [DynamoDBTable("webshop_table")]
    public class Product(string Name,
        string Description,
        string Category,
        decimal Price,
        decimal? DiscountPrice,
        string? ImageUrl,
        string? ThumbnailUrl,
        string Sku) : IModel
    {
        [DynamoDBHashKey("id")]
        public string Id { get; set; } = Category;
        [DynamoDBRangeKey("sort")]
        public string Range { get; set; } = $"PRODUCT#{Sku}";
        [DynamoDBProperty("name")]
        public string Name { get; set; } = Name;
        [DynamoDBProperty("description")]
        public string Description { get; set; } = Description;
        [DynamoDBProperty("price")]
        public decimal Price { get; set; } = Price;
        [DynamoDBProperty("discount_price")]
        public decimal? DiscountPrice { get; set; } = DiscountPrice;
        [DynamoDBProperty("imageUrl")]
        public string? ImageUrl { get; set; } = ImageUrl ?? "https://i.ibb.co/vqLKnHb/Product-inside.png";
        [DynamoDBProperty("thumbnail_url")]
        public string? ThumbnailUrl { get; set; } = ThumbnailUrl;
        [DynamoDBProperty("sku")]
        public string Sku { get; set; } = Sku;
    }
}
