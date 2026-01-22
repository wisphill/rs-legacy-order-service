using LegacyOrderService.Data;
using LegacyOrderService.Models;
using Microsoft.Data.Sqlite;
using Xunit;

namespace LegacyOrderService.tests.Data;

public class OrderRepositoryTests
{
    [Fact]
    public async Task Save_InsertsOrderIntoDatabase()
    {
        // Arrange: Use in-memory SQLite for testing, to open a base connection on RAM
        var connectionString = "Data Source=file:memdb1?mode=memory&cache=shared";
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        // Create Orders table
        var createCmd = connection.CreateCommand();
        createCmd.CommandText = @"
            CREATE TABLE Orders (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                CustomerName TEXT,
                ProductName TEXT,
                Quantity INTEGER,
                Price REAL
            );";
        createCmd.ExecuteNonQuery();

        var repo = new OrderRepositoryForTest(connectionString);

        var order = new Order
        {
            CustomerName = "Alice",
            ProductName = "Book",
            Quantity = 2,
            Price = 19.99m
        };

        // Save create another connection to RAM and then dispose it
        await repo.Save(order, CancellationToken.None);

        // Assert
        var selectCmd = connection.CreateCommand();
        selectCmd.CommandText = "SELECT CustomerName, ProductName, Quantity, Price FROM Orders";
        using var reader = selectCmd.ExecuteReader();
        Assert.True(reader.Read());
        Assert.Equal("Alice", reader.GetString(0));
        Assert.Equal("Book", reader.GetString(1));
        Assert.Equal(2, reader.GetInt32(2));
        Assert.Equal(19.99m, reader.GetDecimal(3));
    }

    // Helper class to inject connection string
    private class OrderRepositoryForTest : OrderRepository
    {
        public OrderRepositoryForTest(string connectionString)
        {
            typeof(OrderRepository)
                .GetField("_connectionString", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(this, connectionString);
        }
    }
}
