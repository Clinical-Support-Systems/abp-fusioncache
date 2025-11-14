using System.ComponentModel.DataAnnotations;

namespace ProductCatalog.Shared.Dtos;

/// <summary>
/// DTO for creating or updating a product.
/// </summary>
public class CreateUpdateProductDto
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Sku { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    public bool IsActive { get; set; } = true;

    [StringLength(100)]
    public string Category { get; set; } = string.Empty;
}
