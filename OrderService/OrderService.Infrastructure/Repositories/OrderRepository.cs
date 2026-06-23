using Microsoft.EntityFrameworkCore;
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

    public async Task<Order?> GetByIdAsync(Guid id)
        => await context.Orders.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id);

    public async Task UpdateAsync(Order order, CancellationToken ct)
    {
        context.Orders.Update(order);
        await context.SaveChangesAsync(ct);
    }
}
