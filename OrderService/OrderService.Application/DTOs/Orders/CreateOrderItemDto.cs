namespace OrderService.Application.DTOs.Orders;

public class CreateOrderItemDto
{
    /// <summary>
    /// Identificador do produto.
    /// </summary>
    /// <example>10</example>
    public int ProductId { get; set; }

    /// <summary>
    /// Quantidade desejada.
    /// </summary>
    /// <example>2</example>
    public int Quantity { get; set; }
}