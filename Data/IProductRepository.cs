namespace LegacyOrderService.Data;

public interface IProductRepository
{
    Task<decimal> GetPrice(string productName);
}