using OrderService.Domain.Enums;
using OrderService.Domain.Models;

namespace OrderService.Domain.Errors;

public static class OrderErrors
{
    public static readonly Error MustContainItems = Error.Validation("Order.MustContainItems", "Um pedido deve conter itens.");
    public static readonly Error NotFound = Error.NotFound("Order.NotFound", "Pedido não encontrado.");
    public static readonly Error InvalidStatusToConfirm = Error.Conflict("Order.InvalidStatusToConfirm", $"Apenas é possível confirmar pedidos com o status de {nameof(OrderStatus.Placed)}.");
    public static readonly Error InvalidStatusToCancel = Error.Conflict("Order.InvalidStatusToCancel", $"Apenas é possível cancelar pedidos com o status de {nameof(OrderStatus.Placed)} ou {nameof(OrderStatus.Confirmed)}.");
}
