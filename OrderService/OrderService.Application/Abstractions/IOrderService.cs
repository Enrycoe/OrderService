using OrderService.Application.DTOs.Orders;
using OrderService.Domain.Models;

namespace OrderService.Application.Abstractions;

public interface IOrderService
{
    public Task<Result<Guid>> CreateAsync(CreateOrderDto orderDto, CancellationToken ct);
    public Task<Result> ConfirmAsync(Guid id, CancellationToken ct);
    public Task<Result<OrderDto>> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Result> CancelAsync(Guid id, CancellationToken ct);
    Task<Result<PagedList<OrderListDto>>> GetAllAsync(int? page, int? pageSize, int? customerId, int? status, DateTime? startDate, DateTime? endDate, CancellationToken ct);
}
