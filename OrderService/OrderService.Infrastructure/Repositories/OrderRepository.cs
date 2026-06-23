using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Abstractions;
using OrderService.Domain.Entities;
using OrderService.Domain.Models;
using OrderService.Infrastructure.Context;
using OrderService.Infrastructure.Extensions;

namespace OrderService.Infrastructure.Repositories;

public class OrderRepository(AppDbContext context) : IOrderRepository
{
    public async Task CreateAsync(Order order, CancellationToken ct)
    {
        await context.Orders.AddAsync(order, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken ct)
        => await context.Orders.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task UpdateAsync(Order order, CancellationToken ct)
    {
        context.Orders.Update(order);
        await context.SaveChangesAsync(ct);
    }

    public async Task<PagedList<Order>> GetAllAsync(int page, int pageSize, DateTime startDate, DateTime endDate, CancellationToken ct)
    {
        var query = context.Orders
            .Where(x => x.CreatedAt >= startDate && x.CreatedAt <= endDate);
        return await query.ToPagedListAsync(page, pageSize, ct);
    }
}
