using LegacyOrderService.Data;
using LegacyOrderService.Models;
using Microsoft.Extensions.Logging;

namespace LegacyOrderService.Services;

public class OrderService(
    IOrderRepository orderRepository, 
    IProductRepository productRepository,
    ILogger<OrderService> logger)
{
    public async Task CreateOrder(Order order, CancellationToken ct)
    {
        logger.LogInformation("Creating an order...");
        // sample validation for the product name
        var hasProduct = await productRepository.HasProduct(order.ProductName);
        if (!hasProduct)
        {
            throw new ArgumentException($"Product name {order.ProductName} does not exist.");
        }
        
        decimal total = order.Quantity * order.Price;
        logger.LogInformation(
            $"Order complete! Customer: {order.CustomerName}, Product: {order.ProductName}, Quantity: {order.Quantity}, Total: ${total}");

        logger.LogInformation("Saving order to database...");
        await orderRepository.Save(order, ct);
    }
}