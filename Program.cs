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
using Serilog;

// TODO: add test cases for multiple writes
// TODO: batching accepted, consider to use csv or json as input for the scalability, all writes into one transaction
// TODO: use the WAL, cache=shared to improve the performance
// TODO: set WAL mode to the orders.db by using another 
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
    
    public class CreateOrderCommand(OrderService orderService, ILogger<CreateOrderCommand> logger) : Command<CreateOrderSettings>
    {
        // TODO: cancellationToken for safe shutdown, do we need to support this
        public override int Execute(CommandContext context, CreateOrderSettings s, CancellationToken cancellationToken)
        {
            DatabaseInitializer.EnsureDatabase();

            // ---- Interactive fallback ----
            var customer = !string.IsNullOrWhiteSpace(s.Customer) 
                ? s.Customer : AnsiConsole.Prompt(
                               new TextPrompt<string>("Customer name:")
                                   .AllowEmpty());;

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
            
            orderService.CreateOrder(order);

            logger.LogInformation("Order created successfully.");
            return 0;
        }
    }


    class Program
    {
        static int Main(string[] args)
        {
            var services = new ServiceCollection();
            SetupLogger(services);
            
            services.AddSingleton<IOrderRepository, OrderRepository>();
            services.AddSingleton<OrderService>();

            var registrar = new CliTypeRegistrar(services);
            var app = new CommandApp(registrar);
            
            app.SetDefaultCommand<CreateOrderCommand>();
            // Use the Configure method to define commands and settings
            app.Configure(config =>
            {
                config.AddCommand<CreateOrderCommand>("create")
                    .WithDescription("Create a new order");
            });

            return app.Run(args);
        }

        static void SetupLogger(IServiceCollection serviceCollections)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
                )
                .WriteTo.File(
                    "logs/app-.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7
                )
                .CreateLogger();
            
            serviceCollections.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddSerilog(dispose: true);
            });
        }
    }
}
