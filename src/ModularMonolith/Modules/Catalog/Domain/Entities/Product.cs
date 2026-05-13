using ModularMonolith.BuildingBlocks.Domain.Abstractions;
using ModularMonolith.BuildingBlocks.Domain.Exceptions;
using ModularMonolith.BuildingBlocks.Domain.Primitives;
using ModularMonolith.Modules.Catalog.Domain.Events;
using ModularMonolith.Modules.Catalog.Domain.Exceptions;

namespace ModularMonolith.Modules.Catalog.Domain.Entities;

public sealed class Product : AggregateRoot, IAuditableEntity, ISoftDeletable
{
    private Product() { }

    public Guid TenantId { get; private set; }
    public Guid CategoryId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string Sku { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int StockQuantity { get; private set; }
    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public Guid? CreatedBy { get; private set; }
    public Guid? UpdatedBy { get; private set; }

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public Guid? DeletedBy { get; private set; }

    public Category Category { get; private set; } = null!;

    public static Product Create(
        Guid tenantId,
        Guid categoryId,
        string name,
        string? description,
        string sku,
        decimal price,
        int stockQuantity = 0)
    {
        if (price < 0)
            throw new DomainException("Price cannot be negative.");
        if (stockQuantity < 0)
            throw new DomainException("Initial stock quantity cannot be negative.");

        var product = new Product
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CategoryId = categoryId,
            Name = name,
            Description = description,
            Sku = sku.ToUpperInvariant(),
            Price = price,
            StockQuantity = stockQuantity,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        product.RaiseDomainEvent(new ProductCreatedDomainEvent(product.Id, tenantId));
        return product;
    }

    public void Update(string name, string? description, decimal price, Guid categoryId)
    {
        if (price < 0)
            throw new DomainException("Price cannot be negative.");

        var oldPrice = Price;
        Name = name;
        Description = description;
        CategoryId = categoryId;
        Price = price;
        UpdatedAt = DateTime.UtcNow;

        if (oldPrice != price)
            RaiseDomainEvent(new ProductPriceChangedDomainEvent(Id, TenantId, oldPrice, price));
    }

    public void AdjustStock(int delta)
    {
        if (StockQuantity + delta < 0)
            throw new InsufficientStockException(Id, StockQuantity, Math.Abs(delta));

        StockQuantity += delta;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new StockAdjustedDomainEvent(Id, TenantId, StockQuantity));
    }

    public void Activate() { IsActive = true; UpdatedAt = DateTime.UtcNow; }
    public void Deactivate() { IsActive = false; UpdatedAt = DateTime.UtcNow; }

    public void SoftDelete(Guid? deletedBy = null)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
    }
}
