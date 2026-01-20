using LegacyOrderService.Models;

namespace LegacyOrderService.Data;

public interface IOrderRepository
{
    void Save(Order order);
}