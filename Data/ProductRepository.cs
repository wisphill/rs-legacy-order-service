// Data/ProductRepository.cs
namespace LegacyOrderService.Data
{
    public class ProductRepository: IProductRepository
    {
        private readonly Dictionary<string, decimal> _productPrices = new()
        {
            ["Widget"] = 12.99m,
            ["Gadget"] = 15.49m,
            ["Doohickey"] = 8.75m
        };

        public async Task<decimal> GetPrice(string productName)
        {
            // Simulate an expensive lookup using async instead blocking the main thread
            await Task.Delay(500);

            if (_productPrices.TryGetValue(productName, out var price))
                return price;

            throw new Exception("Product not found");
        }
        
        public string[] GetProductNames()  => _productPrices.Keys.ToArray();
    }
}
