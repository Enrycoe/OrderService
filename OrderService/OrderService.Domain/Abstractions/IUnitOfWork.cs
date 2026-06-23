namespace OrderService.Domain.Abstractions;

public interface IUnitOfWork
{
    IOrderRepository OrdersRepository { get; }
    IOrderItemRepository OrderItemsRepository { get; }
    IProductRepository ProductsRepository { get; }
    Task CommitAsync(CancellationToken cancellationToken = default);

    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    Task RollbackAsync(CancellationToken cancellationToken = default);
}
