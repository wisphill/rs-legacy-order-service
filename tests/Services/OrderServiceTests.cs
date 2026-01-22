using LegacyOrderService.Data;
using LegacyOrderService.Models;
using LegacyOrderService.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LegacyOrderService.tests.Services;

public class OrderServiceTests
{
    [Fact]
    public async Task CreateOrder_ValidProduct_SavesOrder()
    {
        // Arrange
        var order = new Order { ProductName = "Widget", Quantity = 2, Price = 10, CustomerName = "Alice" };
        var orderRepoMock = new Mock<IOrderRepository>();
        var productRepoMock = new Mock<IProductRepository>();
        var loggerMock = new Mock<ILogger<OrderService>>();
        productRepoMock.Setup(x => x.GetProductNames()).Returns(new[] { "Widget", "Gadget" });

        var service = new OrderService(orderRepoMock.Object, productRepoMock.Object, loggerMock.Object);

        // Act
        await service.CreateOrder(order, CancellationToken.None);

        // Assert
        orderRepoMock.Verify(x => x.Save(order, It.IsAny<CancellationToken>()), Times.Once);
        productRepoMock.Verify(x => x.GetProductNames(), Times.Once);
    }

    [Fact]
    public async Task CreateOrder_InvalidProduct_ThrowsArgumentException()
    {
        // Arrange
        var order = new Order { ProductName = "Unknown", Quantity = 1, Price = 5, CustomerName = null };
        var orderRepoMock = new Mock<IOrderRepository>();
        var productRepoMock = new Mock<IProductRepository>();
        var loggerMock = new Mock<ILogger<OrderService>>();
        productRepoMock.Setup(x => x.GetProductNames()).Returns(new[] { "Widget", "Gadget" });

        var service = new OrderService(orderRepoMock.Object, productRepoMock.Object, loggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateOrder(order, CancellationToken.None));
        orderRepoMock.Verify(x => x.Save(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
