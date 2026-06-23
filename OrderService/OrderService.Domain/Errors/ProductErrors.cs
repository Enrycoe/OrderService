using OrderService.Domain.Models;

namespace OrderService.Domain.Errors;

public static class ProductErrors
{
    public static Error NotFound(int id) => Error.NotFound("Product.NotFound", $"Produto com o id {id} não foi encontrado.");
    public static Error QuantityMustBeGreaterThanZero(int id) => Error.Validation("Product.QuantityMustBeGreaterThanZero", $"A quantidade do produto {id} deve ser maior que zero.");
    public static Error PriceMustBeGreaterThanZero(int id) => Error.Validation("Product.PriceMustBeGreaterThanZero", $"O preço do produto {id} deve ser maior que zero.");
    public static Error InsufficientStock(int id) => Error.Conflict("Product.InsufficientStock", $"Estoque insuficiente para o produto {id}.");
    public static readonly Error StockOperationMustBeWithPositiveQuantity = Error.Validation("Product.StockOperationMustBeWithPositiveQuantity", "Para realizar operações de estoque a quantidade deve ser positiva.");
}
