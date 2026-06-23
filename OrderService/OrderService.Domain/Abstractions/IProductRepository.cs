using OrderService.Domain.Entities;

namespace OrderService.Domain.Abstractions;

public interface IProductRepository
{
    public Task<Product?> GetByIdAsync(int id, CancellationToken ct);
}
