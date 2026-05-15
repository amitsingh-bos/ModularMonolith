using ModularMonolith.BuildingBlocks.Domain.Abstractions;
using ModularMonolith.BuildingBlocks.Domain.Primitives;
using ModularMonolith.Modules.Orders.Domain.Events;
using ModularMonolith.Modules.Orders.Domain.Exceptions;
using IVersionedEntity = ModularMonolith.BuildingBlocks.Domain.Abstractions.IVersionedEntity;

namespace ModularMonolith.Modules.Orders.Domain.Entities;

public sealed class Order : AggregateRoot, IAuditableEntity, ISoftDeletable, IVersionedEntity
{
    private Order() { }

    private readonly List<OrderItem> _items = [];

    public Guid TenantId { get; private set; }
    public Guid CustomerId { get; private set; }
    public string OrderNumber { get; private set; } = string.Empty;
    public OrderStatus Status { get; private set; }
    public string ShippingAddressLine1 { get; private set; } = string.Empty;
    public string? ShippingAddressLine2 { get; private set; }
    public string ShippingCity { get; private set; } = string.Empty;
    public string ShippingCountry { get; private set; } = string.Empty;
    public string ShippingPostalCode { get; private set; } = string.Empty;
    public string? Notes { get; private set; }
    public decimal TotalAmount { get; private set; }
    public int Version { get; private set; } = 1;

    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    // IAuditableEntity
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public Guid? CreatedBy { get; private set; }
    public Guid? UpdatedBy { get; private set; }

    // ISoftDeletable
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public Guid? DeletedBy { get; private set; }

    public static Order Create(
        Guid tenantId,
        Guid customerId,
        string addressLine1,
        string? addressLine2,
        string city,
        string country,
        string postalCode,
        string? notes)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CustomerId = customerId,
            OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}",
            Status = OrderStatus.Pending,
            ShippingAddressLine1 = addressLine1,
            ShippingAddressLine2 = addressLine2,
            ShippingCity = city,
            ShippingCountry = country,
            ShippingPostalCode = postalCode,
            Notes = notes,
            TotalAmount = 0m,
            CreatedAt = DateTime.UtcNow
        };

        order.RaiseDomainEvent(new OrderCreatedDomainEvent(order.Id, tenantId, customerId));
        return order;
    }

    public void AddItem(Guid productId, string productName, string productSku, decimal unitPrice, int quantity)
    {
        var item = OrderItem.Create(Id, productId, productName, productSku, unitPrice, quantity);
        _items.Add(item);
        RecalculateTotal();
        RaiseDomainEvent(new OrderItemAddedDomainEvent(Id, productId, quantity));
    }

    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
            throw new OrderInvalidStatusTransitionException(Id, Status, OrderStatus.Confirmed);

        var oldStatus = Status;
        Status = OrderStatus.Confirmed;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new OrderStatusChangedDomainEvent(Id, TenantId, oldStatus, Status));
    }

    public void Cancel(string? reason = null)
    {
        if (Status is OrderStatus.Shipped or OrderStatus.Delivered or OrderStatus.Refunded)
            throw new OrderInvalidStatusTransitionException(Id, Status, OrderStatus.Cancelled);

        Status = OrderStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new OrderCancelledDomainEvent(Id, TenantId, reason));
    }

    public void Ship()
    {
        if (Status != OrderStatus.Confirmed)
            throw new OrderInvalidStatusTransitionException(Id, Status, OrderStatus.Shipped);

        var oldStatus = Status;
        Status = OrderStatus.Shipped;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new OrderStatusChangedDomainEvent(Id, TenantId, oldStatus, Status));
    }

    public void Deliver()
    {
        if (Status != OrderStatus.Shipped)
            throw new OrderInvalidStatusTransitionException(Id, Status, OrderStatus.Delivered);

        var oldStatus = Status;
        Status = OrderStatus.Delivered;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new OrderStatusChangedDomainEvent(Id, TenantId, oldStatus, Status));
    }

    public void MarkRefunded()
    {
        if (Status != OrderStatus.Delivered)
            throw new OrderInvalidStatusTransitionException(Id, Status, OrderStatus.Refunded);

        Status = OrderStatus.Refunded;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete(Guid? deletedBy = null)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
    }

    private void RecalculateTotal()
    {
        TotalAmount = _items.Sum(i => i.TotalPrice);
    }
}
