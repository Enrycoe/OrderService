using OrderService.Application.Abstractions;
using OrderService.Application.DTOs.Orders;
using OrderService.Domain.Abstractions;
using OrderService.Domain.Entities;
using OrderService.Domain.Errors;
using OrderService.Domain.Models;

namespace OrderService.Application.Services;

public class OrderService(IUnitOfWork unitOfWork) : IOrderService
{
    public async Task<Result<Guid>> CreateAsync(CreateOrderDto orderDto, CancellationToken ct)
    {
        if (orderDto.Items is { Count: 0 })
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
}
