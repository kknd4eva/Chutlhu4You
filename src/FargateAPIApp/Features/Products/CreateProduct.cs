using FargateAPIApp.Features.Products.Models;
using FargateAPIApp.Shared.Models;
using FargateAPIApp.Shared.Repositories.Products;
using FargateWeb.DTOs.ProductDto;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FargateAPIApp.Features.Products
{
    public record CreateProductResponse(ProductDto Product, string Message);
    public record CreateProductCommand(ProductDto Product) : IRequest<CreateProductResponse>;

    public static class CreateProductEndpoint
    {
        public static void MapCreateProductEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapPost("/product", async ([FromBody] ProductDto product,
                                            [FromServices] ISender sender,
                                            [FromServices] IValidator<CreateProductCommand> validator) =>
            {
                try
                {
                    // Create the command
                    var command = new CreateProductCommand(product);

                    var validationResult = await validator.ValidateAsync(command);

                    if (!validationResult.IsValid)
                    {
                        // Return a 400 Bad Request with validation errors
                        var errors = validationResult.Errors
                        .Select(x => new { x.PropertyName, x.ErrorMessage });

                        return Results.BadRequest(new { Errors = errors });
                    }

                    var result = await sender.Send(command);
                    return Results.Created("/product", result);
                }
                catch (Exception ex)
                {
                    // Handle unexpected errors
                    return Results.BadRequest(ex.Message);
                }

            })
            .WithDescription("Create a new product")
            .Accepts<ProductDto>("application/json")
            .WithName("product");
        }
    }

    internal sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductCommandValidator()
        {
            RuleFor(x => x.Product.Name).NotEmpty().WithMessage("Product name is required");
            RuleFor(x => x.Product.Price).GreaterThan(0).WithMessage("Price must be greater than 0");
            RuleFor(x => x.Product.Description).NotEmpty().WithMessage("Description is required");
        }
    }

    internal sealed class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, CreateProductResponse>
    {
        private readonly IProductRepository<Product> _productRepository;
        private readonly ILogger<CreateProductCommandHandler> _logger;
        private readonly IMapper<Product, ProductDto> _productMapper;

        public CreateProductCommandHandler(IProductRepository<Product> productRepository,
            ILogger<CreateProductCommandHandler> logger,
            IMapper<Product, ProductDto> productMapper)
            => (_productRepository, _logger, _productMapper)
            = (productRepository, logger, productMapper);

        public async Task<CreateProductResponse> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Creating product with name {ProductName}", request.Product.Name);

                _productMapper.TryMapToModel(request.Product, out var product);

                await _productRepository.CreateProductAsync(product);

                _productMapper.TryMapToDto(product, out var productDto);

                return new CreateProductResponse(productDto, "Product created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save product {Name} to repository", request.Product.Name);
                throw;
            }
        }
    }
}

