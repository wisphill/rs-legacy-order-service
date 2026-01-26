namespace LegacyOrderService.Data;

public interface IProductRepository
{
    Task<decimal> GetPrice(string productName);
    Task<bool> HasProduct(string productName);
    List<string> SearchByText(string searchTerm);
}