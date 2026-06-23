namespace OrderService.Application.DTOs.Orders;

public class OrderDto
{
    public Guid Id { get; set; }
    public int CustomerId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public List<OrderItemDto> Items { get; set; } = [];
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
}
