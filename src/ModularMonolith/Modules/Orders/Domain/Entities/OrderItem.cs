using ModularMonolith.BuildingBlocks.Domain.Primitives;

namespace ModularMonolith.Modules.Orders.Domain.Entities;

public sealed class OrderItem : Entity
{
    private OrderItem() { }

    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public string ProductSku { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TotalPrice { get; private set; }

    internal static OrderItem Create(
        Guid orderId,
        Guid productId,
        string productName,
        string productSku,
        decimal unitPrice,
        int quantity)
    {
        return new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            ProductId = productId,
            ProductName = productName,
            ProductSku = productSku,
            UnitPrice = unitPrice,
            Quantity = quantity,
            TotalPrice = unitPrice * quantity
        };
    }
}
