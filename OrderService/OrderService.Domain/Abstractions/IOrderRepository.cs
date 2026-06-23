using OrderService.Domain.Entities;

namespace OrderService.Domain.Abstractions;

public interface IOrderRepository
{
    Task CreateAsync(Order order, CancellationToken ct);
}
