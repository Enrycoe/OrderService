using OrderService.Domain.Enums;

namespace OrderService.Application.DTOs.Orders;

public class OrderListDto
{
    public Guid Id { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
