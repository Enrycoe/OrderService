using OrderService.Domain.Enums;
using OrderService.Domain.Errors;
using OrderService.Domain.Models;

namespace OrderService.Domain.Entities;

public class Order
{
    public Guid Id { get; set; }
    public int CustomerId { get; private set; }
    public OrderStatus Status { get; private set; } 
    public string Currency { get; set; } = string.Empty;
    public List<OrderItem> Items { get; private set; } = [];
    public decimal Total { get; private set; }
    public DateTime CreatedAt { get; private set; }
    protected Order() { }

    public static Order Create(int customerId, string currency)
        => new()
        {
            CustomerId = customerId,
            Currency = currency,
            Status = OrderStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            Id = Guid.NewGuid()
        };

    public Result AddItem(int productId, decimal unitPrice, int quantity)
    {

        if (quantity <= 0)
            return Result.Failure(ProductErrors.QuantityMustBeGreaterThanZero(productId));

        if (unitPrice <= 0)
            return Result.Failure(ProductErrors.PriceMustBeGreaterThanZero(productId));

        var existing = Items.FirstOrDefault(i => i.ProductId == productId);

        if (existing is not null)
            existing.IncreaseQuantity(quantity);
        else
            Items.Add(OrderItem.Create(productId, unitPrice, quantity, Id));

        RecalculateTotal();

        return Result.Success();
    }

    private void RecalculateTotal()
        => Total = Items.Sum(i => i.UnitPrice * i.Quantity);

    public void Place()
    {
        Status = OrderStatus.Placed;
    }

    public Result Confirm()
    {
        if (Status == OrderStatus.Confirmed) return Result.Success();

        if (Status != OrderStatus.Placed)
            return Result.Failure(OrderErrors.InvalidStatusToConfirm);

        if (Items is { Count: 0 })
            return Result.Failure(OrderErrors.MustContainItems);

        Status = OrderStatus.Confirmed;
        return Result.Success();
    }

    public Result Cancel()
    {
        if (Status == OrderStatus.Canceled) return Result.Success();

        if (Status is not OrderStatus.Placed and not OrderStatus.Confirmed)
            return Result.Failure(OrderErrors.InvalidStatusToCancel);

        Status = OrderStatus.Canceled;
        return Result.Success();
    }
}
