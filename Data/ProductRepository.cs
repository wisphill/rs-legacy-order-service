// Data/ProductRepository.cs
using System;
using System.Collections.Generic;
using System.Threading;

namespace LegacyOrderService.Data
{
    public class ProductRepository
    {
        // TODO: to check this, should we use double? for finance
        private readonly Dictionary<string, double> _productPrices = new()
        {
            ["Widget"] = 12.99,
            ["Gadget"] = 15.49,
            ["Doohickey"] = 8.75
        };

        public double GetPrice(string productName)
        {
            // Simulate an expensive lookup
            // TODO: check this and consider to use async instead blocking the main thread
            Thread.Sleep(500);

            if (_productPrices.TryGetValue(productName, out var price))
                return price;

            throw new Exception("Product not found");
        }
    }
}
