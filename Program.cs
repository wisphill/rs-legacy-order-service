using System.ComponentModel;
using LegacyOrderService.Models;
using LegacyOrderService.Data;
using LegacyOrderService.Infrastructure;
using LegacyOrderService.Services;
using Spectre.Console.Cli;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using LegacyOrderService.Common;
using Microsoft.Extensions.Logging;

// TODO: add test cases for multiple writes
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
    
    public class CreateOrderCommand(OrderService orderService, ILogger<CreateOrderCommand> logger) : AsyncCommand<CreateOrderSettings>
    {
        // TODO: cancellationToken for safe shutdown, do we need to support this
        public override async Task<int> ExecuteAsync(CommandContext context, CreateOrderSettings s, CancellationToken cancellationToken)
        {
            await Task.Delay(10000, cancellationToken);
            // ---- Interactive fallback ----
            var customer = !string.IsNullOrWhiteSpace(s.Customer) 
                ? s.Customer : AnsiConsole.Prompt(
                               new TextPrompt<string>("Customer name:")
                                   .AllowEmpty());

            var product = s.Product
                          ?? AnsiConsole.Ask<string>("Product name (required):");

            var quantity = s.Quantity
                           ?? AnsiConsole.Ask<int>("Quantity (required):");

            logger.LogInformation("Processing order...");

            var productRepo = new ProductRepository();
            var price = productRepo.GetPrice(product);

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
            
            // Create a cancellation token source to handle Ctrl+C
            var cancellationTokenSource = new CancellationTokenSource();
            // TODO: test this
            // Wire up Console.CancelKeyPress to trigger cancellation
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true; // Prevent immediate process termination
                cancellationTokenSource.Cancel();
                logger.LogInformation("Cancellation requested...");
                Thread.Sleep(5000);
                // force exit and terminate the thread
                e.Cancel = false;
            };
            
            // Wire up to process exit (e.g., SIGTERM, system shutdown)
            AppDomain.CurrentDomain.ProcessExit += (_, _) =>
            {
                cancellationTokenSource.Cancel();
            };
            
            return await app.RunAsync(args, cancellationTokenSource.Token);
        }
    }
}
