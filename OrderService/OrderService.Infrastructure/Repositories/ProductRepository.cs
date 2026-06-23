using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Abstractions;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Context;

namespace OrderService.Infrastructure.Repositories;

public class ProductRepository(AppDbContext context) : IProductRepository
{
    public async Task<Product?> GetByIdAsync(int id, CancellationToken ct)
        => await context.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task UpdateAsync(Product product, CancellationToken ct)
    {
        context.Products.Update(product);
        await context.SaveChangesAsync(ct);
    }
}
