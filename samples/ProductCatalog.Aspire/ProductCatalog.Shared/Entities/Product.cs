using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace ProductCatalog.Shared.Entities;

/// <summary>
/// Represents a product in the catalog.
/// </summary>
public class Product : FullAuditedEntity<int>, IMayHaveTenant
{
    /// <summary>
    /// Tenant ID for multi-tenancy support.
    /// </summary>
    public int? TenantId { get; set; }

    /// <summary>
    /// Product name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Product description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Product SKU (Stock Keeping Unit).
    /// </summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>
    /// Product price.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Available stock quantity.
    /// </summary>
    public int StockQuantity { get; set; }

    /// <summary>
    /// Whether the product is active and visible.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Product category.
    /// </summary>
    public string Category { get; set; } = string.Empty;
}
