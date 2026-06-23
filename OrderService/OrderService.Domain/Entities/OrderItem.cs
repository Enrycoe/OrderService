namespace OrderService.Domain.Entities;

public class OrderItem
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Guid OrderId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    protected OrderItem() { }

    public static OrderItem Create(int productId, decimal unitPrice, int quantity, Guid orderId)
        => new()
        {
            ProductId = productId,
            UnitPrice = unitPrice,
            Quantity = quantity,
            OrderId = orderId
        };

    public void IncreaseQuantity(int quantity)
    {
        Quantity += quantity;
    }
}
