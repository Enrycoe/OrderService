using Microsoft.EntityFrameworkCore.Storage;
using OrderService.Domain.Abstractions;
using OrderService.Infrastructure.Context;

namespace OrderService.Infrastructure.Repositories;

public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    private IDbContextTransaction? _transaction;

    public IOrderRepository OrderRepository => field ??= new OrderRepository(context);
    public IOrderItemRepository OrderItemsRepository => field ??= new OrderItemRepository(context);
    public IProductRepository ProductRepository => field ??= new ProductRepository(context);
    public IUserRepository UserRepository => field ??= new UserRepository(context);

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction ??= await context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);

        if (_transaction is null)
        {
            return;
        }

        await _transaction.CommitAsync(cancellationToken);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null)
        {
            return;
        }

        await _transaction.RollbackAsync(cancellationToken);
        await _transaction.DisposeAsync();
        _transaction = null;
    }
}