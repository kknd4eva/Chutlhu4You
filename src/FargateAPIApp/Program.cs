using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime;
using FargateAPIApp.Features.Orders;
using FargateAPIApp.Features.Orders.DTOs;
using FargateAPIApp.Features.Orders.Models;
using FargateAPIApp.Features.Products;
using FargateAPIApp.Features.Products.Models;
using FargateAPIApp.Shared.Models;
using FargateAPIApp.Shared.Repositories.Orders;
using FargateAPIApp.Shared.Repositories.Products;
using FargateWeb.DTOs.ProductDto;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddConsole();
});
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateOrderCommand).Assembly));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(c =>
    {
        c.Authority = $"https://{builder.Configuration["Auth0:Domain"]}";
        c.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidAudience = builder.Configuration["Auth0:Audience"],
            ValidIssuer = $"{builder.Configuration["Auth0:Domain"]}"
        };
    });
// Register the AWS Message Processing Framework for .NET
builder.Services.AddAWSMessageBus(builder =>
{
    builder.AddSQSPublisher<CreateOrderCommand>(Environment.GetEnvironmentVariable("SQS_PUBLISH_QUEUE") ?? "https://sqs.ap-southeast-2.amazonaws.com/153247006570/PublishSale");
});

builder.Services.AddAuthorization();
builder.Services.AddAuthentication();

var dynamoDbClient = new AmazonDynamoDBClient(
    FallbackCredentialsFactory.GetCredentials(), RegionEndpoint.APSoutheast2);
builder.Services.AddSingleton<IAmazonDynamoDB>(dynamoDbClient);
builder.Services.AddSingleton<IDynamoDBContext, DynamoDBContext>();
builder.Services.AddScoped<IValidator<CreateOrderCommand>, CreateOrderCommandValidator>();

// add repositories
builder.Services.AddScoped<IOrderRepository<Order>, OrderRepository>();
builder.Services.AddScoped<IProductRepository<Product>, ProductRepository>();
builder.Services.AddTransient<IMapper<Order, OrderDto>,  OrderMapper>();
builder.Services.AddTransient<IMapper<Product, ProductDto>, ProductMapper>();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapCreateOrderEndpoint();
app.MapGetOrderEndpoint();
app.MapCreateProductEndpoint();

// create a health check endpoint that returns "health" and a 200 status code
app.MapGet("/health", () =>
{
    return Results.Ok();
})
.WithOpenApi()
.WithName("health")
.AllowAnonymous();

app.Run();
