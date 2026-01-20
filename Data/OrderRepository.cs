using LegacyOrderService.Infrastructure;
using Microsoft.Data.Sqlite;
using LegacyOrderService.Models;

namespace LegacyOrderService.Data
{
    public class OrderRepository
    {
        private readonly string _connectionString = $"Data Source={DatabaseInitializer.DbPath}";

        public void Save(Order order)
        {
            // auto dispose the connection
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = """
                                  INSERT INTO Orders (CustomerName, ProductName, Quantity, Price)
                                  VALUES ($customerName, $productName, $quantity, $price);
                                  """;

            command.Parameters.AddWithValue("$customerName", order.CustomerName);
            command.Parameters.AddWithValue("$productName", order.ProductName);
            command.Parameters.AddWithValue("$quantity", order.Quantity);
            command.Parameters.AddWithValue("$price", order.Price);

            command.ExecuteNonQuery();
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
