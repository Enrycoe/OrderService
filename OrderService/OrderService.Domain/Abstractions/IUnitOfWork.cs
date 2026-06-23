namespace OrderService.Domain.Abstractions;

public interface IUnitOfWork
{
    IOrderRepository OrderRepository { get; }
    IOrderItemRepository OrderItemsRepository { get; }
    IProductRepository ProductRepository { get; }
    IUserRepository UserRepository { get; }
    Task CommitAsync(CancellationToken cancellationToken = default);

    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    Task RollbackAsync(CancellationToken cancellationToken = default);
}
