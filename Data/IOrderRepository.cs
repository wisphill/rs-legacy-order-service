using LegacyOrderService.Models;

namespace LegacyOrderService.Data;

public interface IOrderRepository
{ 
    Task Save(Order order, CancellationToken ct);
}