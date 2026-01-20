using LegacyOrderService.Data;
using LegacyOrderService.Models;

namespace LegacyOrderService.Services;

public class OrderService(IOrderRepository orderRepository)
{
    public void CreateOrder(Order order)
    {
        // TODO: add validation here before saving to the database
        decimal total = order.Quantity * order.Price;

        Console.WriteLine("Order complete!");
        Console.WriteLine("Customer: " + order.CustomerName);
        Console.WriteLine("Product: " + order.ProductName);
        Console.WriteLine("Quantity: " + order.Quantity);
        Console.WriteLine("Total: $" + total);

        // TODO: replace the Console with logger >> log to file
        Console.WriteLine("Saving order to database...");
        orderRepository.Save(order);
    }
}