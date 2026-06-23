using System.ComponentModel.DataAnnotations;

namespace OrderService.Application.DTOs.Orders;

public class CreateOrderDto
{
    public int CustomerId { get; set; }

    [MaxLength(3)]
    public string Currency { get; set; } = string.Empty;
    public List<CreateOrderItemDto> Items { get; set; } = [];
}
