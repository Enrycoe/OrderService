using OrderService.Application.DTOs.Orders;
using OrderService.Domain.Models;

namespace OrderService.Application.Abstractions;

public interface IOrderService
{
    public Task<Result<Guid>> CreateAsync(CreateOrderDto orderDto, CancellationToken ct);
    public Task<Result> ConfirmAsync(Guid id, CancellationToken ct);
}
