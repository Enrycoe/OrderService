using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Abstractions;
using OrderService.Domain.Entities;
using OrderService.Domain.Models;
using OrderService.Infrastructure.Context;
using OrderService.Infrastructure.Extensions;
using Org.BouncyCastle.Asn1.Ocsp;

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

    public async Task<PagedList<Order>> GetAllAsync(int? page, int? pageSize, DateTime? startDate, DateTime? endDate, CancellationToken ct)
    {
        var query = context.Orders.AsQueryable();

        if (startDate.HasValue)
        {
            startDate = DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc);
            query = query.Where(x => x.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            endDate = DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc);
            query = query.Where(x => x.CreatedAt <= endDate.Value);
        }

        query = query.OrderByDescending(x => x.CreatedAt).ThenBy(x => x.Id);  

        return await query.ToPagedListAsync(page, pageSize, ct);
    }
}
