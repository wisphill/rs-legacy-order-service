using LegacyOrderService.Infrastructure;
using Microsoft.Data.Sqlite;
using LegacyOrderService.Models;

namespace LegacyOrderService.Data
{
    public class OrderRepository : IOrderRepository
    {
        private readonly string _connectionString = $"Data Source={DatabaseInitializer.DbPath}";

        public async Task Save(Order order, CancellationToken ct)
        {
            // auto dispose the connection
            await using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync(ct);

            await using var command = connection.CreateCommand();
            command.CommandText = """
                                  INSERT INTO Orders (CustomerName, ProductName, Quantity, Price)
                                  VALUES ($customerName, $productName, $quantity, $price);
                                  """;

            command.Parameters.AddWithValue("$customerName", order.CustomerName);
            command.Parameters.AddWithValue("$productName", order.ProductName);
            command.Parameters.AddWithValue("$quantity", order.Quantity);
            command.Parameters.AddWithValue("$price", order.Price);

            await command.ExecuteNonQueryAsync(ct);
        }

        public void SeedBadData()
        {
            using var connection = new SqliteConnection(_connectionString);            
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO Orders (CustomerName, ProductName, Quantity, Price) VALUES ('John', 'Widget', 9999, 9.99)";
            cmd.ExecuteNonQuery();
        }
    }
}
