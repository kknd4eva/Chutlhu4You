using Amazon.Extensions.NETCore.Setup;
using FargateOrderProcessor.Entities;
using FargateOrderProcessor.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TinyHealthCheck;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        AWSOptions awsOptions = context.Configuration.GetAWSOptions();
        services.AddDefaultAWSOptions(awsOptions);
        services.AddBasicTinyHealthCheck(config =>
        {
            config.Port = 8080;
            config.UrlPath = "/health";
            return config;
        });
        services.AddAWSMessageBus(builder =>
        {
            builder.AddSQSPoller(context.Configuration.GetValue<string>("SQS_PUBLISH_QUEUE"));
            builder.AddMessageHandler<OrderService, CreateOrderCommand>("FargateAPIApp.Features.Orders.CreateOrderCommand");
        });
    })
    .ConfigureLogging(logging =>
    {
        // Configure logging to help with debugging
        logging.ClearProviders();
        logging.AddConsole();
        logging.SetMinimumLevel(LogLevel.Debug);
    })
    .Build();

await host.RunAsync();
