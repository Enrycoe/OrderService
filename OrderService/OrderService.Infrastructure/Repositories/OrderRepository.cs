using OrderService.Domain.Abstractions;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Context;

namespace OrderService.Infrastructure.Repositories;

public class OrderRepository(AppDbContext context) : IOrderRepository
{
    public async Task CreateAsync(Order order, CancellationToken ct)
    {
        await context.Orders.AddAsync(order, ct);
        await context.SaveChangesAsync(ct);
    }
}
