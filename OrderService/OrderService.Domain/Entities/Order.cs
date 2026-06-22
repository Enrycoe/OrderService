using OrderService.Domain.Enums;
using System.Numerics;

namespace OrderService.Domain.Entities;

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public OrderStatus Status { get; set; } 
    public string Currency { get; set; } = string.Empty;
    public List<OrderItem> Items { get; set; } 
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
}
