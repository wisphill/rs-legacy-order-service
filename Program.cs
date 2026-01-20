using System.ComponentModel;
using LegacyOrderService.Models;
using LegacyOrderService.Data;
using LegacyOrderService.Infrastructure;
using LegacyOrderService.Services;
using Spectre.Console.Cli;
using System.ComponentModel.DataAnnotations;
using Spectre.Console;

// TODO: Move these things to the service layer following micro-service design
// we have: Cli, Application service layer, Repository layer, Data model layer
// TODO: add test cases for multiple writes
// TODO: Do we really need a mutex lock when writing to the SQLite database?
// TODO: consider an well-known package for handing the args
// TODO: add args validation
// TODO: add helper text for the man guide
// TODO: batching accepted, consider to use csv or json as input for the scalability
// TODO: add all writes into one transaction
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
    
    public class CreateOrderCommand : Command<CreateOrderSettings>
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

            Console.WriteLine("Processing order...");

            var productRepo = new ProductRepository();
            var price = productRepo.GetPrice(product);

            var order = new Order
            {
                CustomerName = customer,
                ProductName = product,
                Quantity = quantity,
                Price = price
            };

            var repo = new OrderRepository();
            var service = new OrderService(repo);
            service.CreateOrder(order);

            Console.WriteLine("Order created successfully.");
            return 0;
        }
    }


    class Program
    {
        static int Main(string[] args)
        {
            var app = new CommandApp();
            app.SetDefaultCommand<CreateOrderCommand>();
            // Use the Configure method to define commands and settings
            app.Configure(config =>
            {
                config.AddCommand<CreateOrderCommand>("create")
                    .WithDescription("Create a new order");
            });

            return app.Run(args);
        }
    }
}
