namespace LegacyOrderService.tests.Data;

using System;
using System.Threading.Tasks;
using LegacyOrderService.Data;
using Xunit;

public class ProductRepositoryTests
{
    [Fact]
    public async Task GetPrice_ReturnsCorrectPrice_ForKnownProduct()
    {
        var repo = new ProductRepository();

        var price = await repo.GetPrice("Widget");

        Assert.Equal(12.99m, price);
    }

    [Fact]
    public async Task GetPrice_ThrowsException_ForUnknownProduct()
    {
        var repo = new ProductRepository();

        await Assert.ThrowsAsync<Exception>(() => repo.GetPrice("NonExistentProduct"));
    }
}
