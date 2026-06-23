using OrderService.Domain.Abstractions;
using OrderService.Infrastructure.Context;

namespace OrderService.Infrastructure.Repositories;

public class ProductRepository(AppDbContext context) : IProductRepository
{
}
