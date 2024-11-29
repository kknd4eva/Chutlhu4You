using FargateWeb.DTOs.ProductDto;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FargateAPIApp.Features.Products
{
    public record CreateProductResponse(ProductDto Product, string Message);
    public record CreateProductRequest(ProductDto Product) : IRequest<CreateProductResponse>;

    public static class CreateProductEndpoint
    {
        public static void MapCreateProductEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapPost("/product", async ([FromBody] ProductDto product,
                                            [FromServices] ISender sender,
                                            [FromServices] IValidator<CreateProductRequest> validator) =>
            {
                try
                {
                    // Create the command
                    var command = new CreateProductRequest(product);

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
}

