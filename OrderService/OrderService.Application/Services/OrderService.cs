using OrderService.Application.Abstractions;
using OrderService.Application.DTOs.Orders;
using OrderService.Domain.Abstractions;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Domain.Errors;
using OrderService.Domain.Models;

namespace OrderService.Application.Services;

public class OrderService(IUnitOfWork unitOfWork) : IOrderService
{
    public async Task<Result> ConfirmAsync(Guid id, CancellationToken ct)
    {
        var orderResult = await GetOrderAsync(id, ct);
        if (orderResult.IsFailure)
            return Result.Failure(orderResult.Errors);

        var order = orderResult.Data!;

        if (order.Status is OrderStatus.Confirmed)
            return Result.Success();

        var confirmResult = order.Confirm();
        if (confirmResult.IsFailure)
            return confirmResult;

        return await ExecuteTransactionAsync(
            async () =>
            {
                var stockResult = await UpdateStockAsync(
                    order,
                    (product, quantity) => product.RemoveStock(quantity),
                    ct);

                if (stockResult.IsFailure)
                    return stockResult;

                await unitOfWork.OrderRepository.UpdateAsync(order, ct);

                return Result.Success();
            },
            ct);
    }

    public async Task<Result> CancelAsync(Guid id, CancellationToken ct)
    {
        var orderResult = await GetOrderAsync(id, ct);
        if (orderResult.IsFailure)
            return Result.Failure(orderResult.Errors);

        var order = orderResult.Data!;

        if (order.Status is OrderStatus.Canceled)
            return Result.Success();

        return await ExecuteTransactionAsync(
            async () =>
            {
                if (order.Status is OrderStatus.Confirmed)
                {
                    var stockResult = await UpdateStockAsync(
                        order,
                        (product, quantity) => product.AddStock(quantity),
                        ct);

                    if (stockResult.IsFailure)
                        return stockResult;
                }

                var cancelResult = order.Cancel();
                if (cancelResult.IsFailure)
                    return cancelResult;

                await unitOfWork.OrderRepository.UpdateAsync(order, ct);

                return Result.Success();
            },
            ct);
    }

    private async Task<Result<Order>> GetOrderAsync(Guid id, CancellationToken ct)
    {
        var order = await unitOfWork.OrderRepository.GetByIdAsync(id, ct);

        return order is null
            ? Result<Order>.Failure(OrderErrors.NotFound)
            : Result.Success(order);
    }

    private async Task<Result> UpdateStockAsync(
        Order order,
        Func<Product, int, Result> stockOperation,
        CancellationToken ct)
    {
        foreach (var item in order.Items)
        {
            var product = await unitOfWork.ProductRepository.GetByIdAsync(item.ProductId, ct);

            if (product is null)
                return Result.Failure(ProductErrors.NotFound(item.ProductId));

            var stockResult = stockOperation(product, item.Quantity);

            if (stockResult.IsFailure)
                return stockResult;

            await unitOfWork.ProductRepository.UpdateAsync(product, ct);
        }

        return Result.Success();
    }

    private async Task<Result> ExecuteTransactionAsync(
        Func<Task<Result>> action,
        CancellationToken ct)
    {
        try
        {
            await unitOfWork.BeginTransactionAsync(ct);

            var result = await action();

            if (result.IsFailure)
            {
                await unitOfWork.RollbackAsync(ct);
                return result;
            }

            await unitOfWork.CommitAsync(ct);

            return result;
        }
        catch
        {
            await unitOfWork.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<Result<Guid>> CreateAsync(CreateOrderDto orderDto, CancellationToken ct)
    {
        if (orderDto.Items is not { Count: > 0 })
            return Result<Guid>.Failure(OrderErrors.MustContainItems);

        var order = Order.Create(orderDto.CustomerId, orderDto.Currency);

        foreach (var itemDto in orderDto.Items)
        {
            var product = await unitOfWork.ProductRepository.GetByIdAsync(itemDto.ProductId, ct);

            if (product is null)
                return Result<Guid>.Failure(ProductErrors.NotFound(itemDto.ProductId));

            if (product.AvailableStock < itemDto.Quantity)
                return Result<Guid>.Failure(ProductErrors.InsufficientStock(itemDto.ProductId));

            var addResult = order.AddItem(product.Id, product.UnitPrice, itemDto.Quantity);
            if (addResult.IsFailure)
                return Result<Guid>.Failure(addResult.Errors);
        }

        order.Place();
        await unitOfWork.OrderRepository.CreateAsync(order, ct);

        return Result.Success(order.Id);
    }

    public async Task<Result<PagedList<OrderListDto>>> GetAllAsync(int? page, int? pageSize, DateTime? startDate, DateTime? endDate, CancellationToken ct)
    {
        var pagedResult = await unitOfWork.OrderRepository.GetAllAsync(page, pageSize, startDate, endDate, ct);
        var pagedResultDto = new PagedList<OrderListDto>
        {
            Data = [.. pagedResult.Data.Select(o => new OrderListDto
            {
                Id = o.Id,
                Total = o.Total,
                Status = o.Status.ToString(),
                CreatedAt = o.CreatedAt
            })],
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize,
            TotalCount = pagedResult.TotalCount
        };

        return Result.Success(pagedResultDto);
    }

    public async Task<Result<OrderDto>> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var order = await unitOfWork.OrderRepository.GetByIdAsync(id, ct);
        if (order is null)
            return Result<OrderDto>.Failure(OrderErrors.NotFound);

        var orderDto = new OrderDto
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            Currency = order.Currency,
            Total = order.Total,
            Status = order.Status.ToString(),
            CreatedAt = order.CreatedAt,
            Items = [.. order.Items.Select(i => new OrderItemDto
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            })]
        };

        return Result.Success(orderDto);
    }
}