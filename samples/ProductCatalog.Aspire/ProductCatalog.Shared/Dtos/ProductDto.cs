using Abp.Application.Services.Dto;

namespace ProductCatalog.Shared.Dtos;

/// <summary>
/// DTO for Product entity.
/// </summary>
public class ProductDto : EntityDto<int>
{
    public int? TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; }
    public string Category { get; set; } = string.Empty;
    public DateTime CreationTime { get; set; }
    public DateTime? LastModificationTime { get; set; }
}
