using LegacyOrderService.Data;
using LegacyOrderService.Models;
using Microsoft.Extensions.Logging;

namespace LegacyOrderService.Services;

public class OrderService(IOrderRepository orderRepository, ILogger<OrderService> logger)
{
    public async Task CreateOrder(Order order, CancellationToken ct)
    {
        // TODO: add validation here before saving to the database
        decimal total = order.Quantity * order.Price;
        logger.LogInformation("Order complete!");
        logger.LogInformation("Customer: " + order.CustomerName);
        logger.LogInformation("Product: " + order.ProductName);
        logger.LogInformation("Quantity: " + order.Quantity);
        logger.LogInformation("Total: $" + total);

        logger.LogInformation("Saving order to database...");
        await orderRepository.Save(order, ct);
    }
}