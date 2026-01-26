namespace LegacyOrderService.Models
{
    // More order information can be added and modified later
    public class Order
    {
        public string? CustomerName { get; set; }
        // required because it's needed for searching product price
        public required string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
