using OrderService.Domain.Errors;
using OrderService.Domain.Models;

namespace OrderService.Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int AvailableStock { get; set; }

    public Result RemoveStock(int quantity)
    {
        if (quantity <= 0)
            return Result.Failure(ProductErrors.StockOperationMustBeWithPositiveQuantity);

        if (AvailableStock < quantity)
            return Result.Failure(ProductErrors.InsufficientStock(Id));

        AvailableStock -= quantity;
        return Result.Success();
    }

    public Result AddStock(int quantity)
    {
        if (quantity <= 0)
            return Result.Failure(ProductErrors.StockOperationMustBeWithPositiveQuantity);

        AvailableStock += quantity;
        return Result.Success();
    }
}
