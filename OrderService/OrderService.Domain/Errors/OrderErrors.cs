using OrderService.Domain.Models;

namespace OrderService.Domain.Errors;

public static class OrderErrors
{
    public static readonly Error MustContainItems = Error.Validation("Order.MustContainItems", "Um pedido deve conter itens.");
}
