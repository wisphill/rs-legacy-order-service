using System;
using LegacyOrderService.Models;
using LegacyOrderService.Data;

namespace LegacyOrderService
{
    class Program
    {
        static void Main(string[] args)
        {
            // TODO: Move these things to the service layer following micro-service design
            // we have: Cli, Application service layer, Repository layer, Data model layer
            // TODO: add test cases for multiple writes
            
            // TODO: Use a for-loop for creating an order
            // TODO: Do we really need a mutex lock when writing to the SQLite database?
            
            // TODO: consider an well-known package for handing the args
            // TODO: add args validation
            // TODO: add helper text for the man guide
            
            // TODO: batching accepted, consider to use csv or json as input for the scalability
            
            // TODO: add all writes into one transaction
            
            // TODO: use the WAL, cache=shared to improve the performance
            
            // TODO: set WAL mode to the orders.db by using another 
            
            // TODO: add script to have a daily backup the db? generate bak files
            Console.WriteLine("Welcome to Order Processor!");
            Console.WriteLine("Enter customer name:");
            string name = Console.ReadLine();

            Console.WriteLine("Enter product name:");
            string product = Console.ReadLine();
            var productRepo = new ProductRepository();
            double price = productRepo.GetPrice(product);


            Console.WriteLine("Enter quantity:");
            int qty = Convert.ToInt32(Console.ReadLine());

            // TODO: expose this into another function to call "Processing order" to be implemented later
            Console.WriteLine("Processing order...");

            Order order = new Order();
            
            // TODO: fix name can be null
            order.CustomerName = name;
            order.ProductName = product;
            order.Quantity = qty;
            order.Price = 10.0;

            // TODO: type re-checking for the order, total
            // TODO: fix total is unused
            double total = order.Quantity * order.Price;

            Console.WriteLine("Order complete!");
            Console.WriteLine("Customer: " + order.CustomerName);
            Console.WriteLine("Product: " + order.ProductName);
            Console.WriteLine("Quantity: " + order.Quantity);
            Console.WriteLine("Total: $" + price);

            // TODO: replace the Console with logger >> log to file
            Console.WriteLine("Saving order to database...");
            var repo = new OrderRepository();
            repo.Save(order);
            Console.WriteLine("Done.");
        }
    }
}
