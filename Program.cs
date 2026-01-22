using System.ComponentModel;
using LegacyOrderService.Models;
using LegacyOrderService.Data;
using LegacyOrderService.Infrastructure;
using LegacyOrderService.Services;
using Spectre.Console.Cli;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using LegacyOrderService.Common;
using Microsoft.Extensions.Logging;

// TODO: batching accepted, consider to use csv or json as input for the scalability, all writes into one transaction
// TODO: add script to have a daily backup the db? generate bak files
namespace LegacyOrderService
{
    public class CreateOrderSettings : CommandSettings
    {
        [CommandOption("--customer <NAME>")]
        [DefaultValue("")]
        public string? Customer { get; set; }

        [CommandOption("--product <NAME>")]
        public string? Product { get; set; }

        [CommandOption("--quantity <QTY>")]
        [Range(1, int.MaxValue)]
        public int? Quantity { get; set; }
    }
    
    public class CreateOrderCommand(
        OrderService orderService, 
        IProductRepository productRepository,
        ILogger<CreateOrderCommand> logger) : AsyncCommand<CreateOrderSettings>
    {
        public override async Task<int> ExecuteAsync(CommandContext context, CreateOrderSettings s, CancellationToken cancellationToken)
        {
            // ---- Interactive fallback ----
            var customer = !string.IsNullOrWhiteSpace(s.Customer) 
                ? s.Customer : AnsiConsole.Prompt(
                               new TextPrompt<string>("Customer name (optional):")
                                   .AllowEmpty());

            var product = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select your [green]product[/]?")
                    .AddChoices(productRepository.GetProductNames()));
            AnsiConsole.MarkupLine($"Selected Product: [yellow]{product}[/]");

            var quantity = s.Quantity
                           ?? AnsiConsole.Ask<int>("Quantity (required):");

            logger.LogInformation("Processing order...");

            var price = await productRepository.GetPrice(product);

            var order = new Order
            {
                CustomerName = customer,
                ProductName = product,
                Quantity = quantity,
                Price = price
            };
            
            await orderService.CreateOrder(order, cancellationToken);
            logger.LogInformation("Order created successfully.");
            return 0;
        }
    }


  class Program
    {
        static async Task<int> Main(string[] args)
        {
            var services = new ServiceCollection();
            
            services
                .AddLogger()
                .AddSingleton<IOrderRepository, OrderRepository>()
                .AddSingleton<IProductRepository, ProductRepository>()
                .AddSingleton<OrderService>()
                .AddSingleton<DatabaseInitializer>();
        
            var registrar = new CliTypeRegistrar(services);
            var app = new CommandApp(registrar);
            
            app.SetDefaultCommand<CreateOrderCommand>();
            // Use the Configure method to define commands and settings
            app.Configure(config =>
            {
                config.AddCommand<CreateOrderCommand>("create")
                    .WithDescription("Create a new order");
            });
            
            var serviceProvider = services.BuildServiceProvider();
            var logger = serviceProvider
                .GetRequiredService<ILogger<Program>>();
            var databaseInitializer = serviceProvider
                .GetRequiredService<DatabaseInitializer>();
            databaseInitializer.EnsureDatabase();
        
            logger.LogInformation("Application started");
            
            // Create a cancellation token source
            var cancellationTokenSource = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) =>
            {
                logger.LogInformation("Cancellation requested. Gracefully shutting down application...");
                GracefulShutdown(cancellationTokenSource);
            };
            
            PosixSignalRegistration.Create(PosixSignal.SIGTERM, ctx =>
            {
                logger.LogInformation("Getting SIGTERM. Gracefully shutting down application...");
                GracefulShutdown(cancellationTokenSource);
            });

            return await app.RunAsync(args, cancellationTokenSource.Token);
        }

        static void GracefulShutdown(CancellationTokenSource cancellationTokenSource)
        {
            cancellationTokenSource.Cancel();
            Thread.Sleep(AppConstants.GracefulShutdownTimeoutMilliSecs);
        }
    }
}
