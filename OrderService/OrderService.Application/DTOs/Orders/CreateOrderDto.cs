using System.ComponentModel.DataAnnotations;

namespace OrderService.Application.DTOs.Orders;

public class CreateOrderDto
{
    /// <summary>
    /// Identificador do cliente.
    /// </summary>
    /// <example>123</example>
    public int CustomerId { get; set; }

    /// <summary>
    /// Código da moeda ISO-4217.
    /// </summary>
    /// <example>BRL</example>
    [MaxLength(3)]
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Itens do pedido.
    /// </summary>
    public List<CreateOrderItemDto> Items { get; set; } = [];
}
