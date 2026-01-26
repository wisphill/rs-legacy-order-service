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
        
        public async Task<List<string>> SearchByText(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await Task.FromResult(_productPrices.Keys.Take(10).ToList());

            return await Task.FromResult(_productPrices.Keys
                .Where(key => key.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList());
        }

        
        public async Task<bool> HasProduct(string productName)
        {
            await Task.Delay(100); // Simulate async work
            return _productPrices.ContainsKey(productName);
        }
    }
}
