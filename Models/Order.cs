namespace LegacyOrderService.Models
{
    // More order information can be added and modified later
    public class Order
    {
        public string? CustomerName;
        // required because it's needed for searching product price
        public required string ProductName;
        public int Quantity;
        public decimal Price;
    }
}
