namespace OrderService.Application.DTOs.Orders;

public class OrderItemDto
{
    public int ProductId { get; set; }
    public Guid OrderId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}