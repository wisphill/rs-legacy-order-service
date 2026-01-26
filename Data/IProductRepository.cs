namespace LegacyOrderService.Data;

public interface IProductRepository
{
    Task<decimal> GetPrice(string productName);
    Task<bool> HasProduct(string productName);
    Task<List<string>> SearchByText(string searchTerm);
}