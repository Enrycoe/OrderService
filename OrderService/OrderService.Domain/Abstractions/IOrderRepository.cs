using OrderService.Domain.Entities;
using OrderService.Domain.Models;

namespace OrderService.Domain.Abstractions;

public interface IOrderRepository
{
    Task CreateAsync(Order order, CancellationToken ct);
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct);
    Task UpdateAsync(Order order, CancellationToken ct);
    Task<PagedList<Order>> GetAllAsync(int? page, int? pageSize, DateTime? startDate, DateTime? endDate, CancellationToken ct);
}
