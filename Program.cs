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

// Some features can be added later:
// 1. batching, consider to use csv or json as input for the scalability, all writes into one transaction
// 2. add script to have a daily backup the db to generate bak files
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

            string? product = null;
            if (s.Product != null)
            {
                product = s.Product;
            }
            else
            {
                var initialChoices = (await productRepository.SearchByText(string.Empty).ConfigureAwait(false)).ToList();
                initialChoices.Insert(0, "([grey]Search again...[/])");
                while (product == null)
                {
                    var selection = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("Select a product or search:")
                            .PageSize(10)
                            .AddChoices(initialChoices));

                    if (selection != "([grey]Search again...[/])")
                    {
                        product = selection;
                        break;
                    }

                    var search = AnsiConsole.Prompt(
                        new TextPrompt<string>("[bold]Search product name:[/] ")
                            .AllowEmpty());
                    var matches = await productRepository.SearchByText(search).ConfigureAwait(false);

                    if (!matches.Any())
                    {
                        AnsiConsole.MarkupLine("[red]⚠ No matches in database. Please refine your search.[/]");
                        continue;
                    }

                    var choices = matches.ToList();
                    choices.Insert(0, "([grey]Search again...[/])");

                    selection = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title($"Found [green]{matches.Count}[/] matches. Select one:")
                            .PageSize(10)
                            .AddChoices(choices));

                    if (selection != "([grey]Search again...[/])")
                    {
                        product = selection;
                    }
                }

                AnsiConsole.MarkupLine($"Confirmed: [yellow]{product}[/]");
            }

            var quantity = s.Quantity
                           ?? AnsiConsole.Ask<int>("Quantity (required):");

            logger.LogInformation("Processing order...");

            var price = await productRepository.GetPrice(product).ConfigureAwait(false);

            var order = new Order
            {
                CustomerName = customer,
                ProductName = product,
                Quantity = quantity,
                Price = price
            };
            
            await orderService.CreateOrder(order, cancellationToken).ConfigureAwait(false);
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
        
            logger.LogInformation("Console application started");
            
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
