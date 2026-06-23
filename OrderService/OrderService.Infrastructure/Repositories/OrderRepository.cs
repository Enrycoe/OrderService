using OrderService.Domain.Abstractions;
using OrderService.Infrastructure.Context;

namespace OrderService.Infrastructure.Repositories;

public class OrderRepository(AppDbContext context) : IOrderRepository
{
}
