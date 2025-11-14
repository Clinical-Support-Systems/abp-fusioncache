using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Runtime.Caching;
using Abp.Runtime.Session;
using ProductCatalog.Shared.Dtos;
using ProductCatalog.Shared.Entities;

namespace ProductCatalog.Admin.Services;

/// <summary>
/// Service for managing products with cache invalidation.
/// Demonstrates Redis backplane synchronization across services.
/// </summary>
public class ProductAdminService : ITransientDependency
{
    private readonly ICacheManager _cacheManager;
    private readonly IRepository<Product, int> _productRepository;
    private readonly IAbpSession _session;
    private readonly ILogger<ProductAdminService> _logger;

    public ProductAdminService(
        ICacheManager cacheManager,
        IRepository<Product, int> productRepository,
        IAbpSession abpSession,
        ILogger<ProductAdminService> logger)
    {
        _cacheManager = cacheManager;
        _productRepository = productRepository;
        _session = abpSession;
        _logger = logger;
    }

    /// <summary>
    /// Gets all products for admin view (no caching for admin to see real-time data).
    /// </summary>
    public async Task<List<ProductDto>> GetAllProductsAsync()
    {
        _logger.LogInformation("Admin: Getting all products for tenant {TenantId}", _session.TenantId);

        var entities = await _productRepository.GetAllListAsync();
        return entities.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Gets a single product for editing.
    /// </summary>
    public async Task<ProductDto?> GetProductAsync(int productId)
    {
        _logger.LogInformation("Admin: Getting product {ProductId}", productId);

        var entity = await _productRepository.FirstOrDefaultAsync(p => p.Id == productId);
        return entity != null ? MapToDto(entity) : null;
    }

    /// <summary>
    /// Creates a new product and invalidates the list cache.
    /// The backplane will notify API instances to invalidate their cache.
    /// </summary>
    public async Task<ProductDto> CreateProductAsync(CreateUpdateProductDto input)
    {
        _logger.LogInformation("Admin: Creating new product {Name}", input.Name);

        var product = new Product
        {
            TenantId = _session.TenantId,
            Name = input.Name,
            Description = input.Description,
            Sku = input.Sku,
            Price = input.Price,
            StockQuantity = input.StockQuantity,
            IsActive = input.IsActive,
            Category = input.Category
        };

        await _productRepository.InsertAsync(product);

        // Invalidate list caches - backplane will sync to API instances
        await InvalidateListCachesAsync();

        _logger.LogInformation("Admin: Created product {ProductId}, invalidated caches", product.Id);

        return MapToDto(product);
    }

    /// <summary>
    /// Updates an existing product and invalidates related caches.
    /// Demonstrates how cache invalidation propagates via Redis backplane.
    /// </summary>
    public async Task<ProductDto> UpdateProductAsync(int id, CreateUpdateProductDto input)
    {
        _logger.LogInformation("Admin: Updating product {ProductId}", id);

        var product = await _productRepository.GetAsync(id);

        // Update properties
        product.Name = input.Name;
        product.Description = input.Description;
        product.Sku = input.Sku;
        product.Price = input.Price;
        product.StockQuantity = input.StockQuantity;
        product.IsActive = input.IsActive;
        product.Category = input.Category;

        await _productRepository.UpdateAsync(product);

        // Invalidate specific product cache + list caches
        // The Redis backplane broadcasts this to all connected instances
        var cache = _cacheManager.GetCache("ProductCache");

        _logger.LogInformation(
            "Admin: Invalidating cache for product {ProductId} and category {Category}",
            id,
            product.Category);

        await cache.RemoveAsync(id.ToString());
        await cache.RemoveAsync($"category:{product.Category}");
        await InvalidateListCachesAsync();

        _logger.LogInformation("Admin: Updated product {ProductId}, caches invalidated and synced via backplane", id);

        return MapToDto(product);
    }

    /// <summary>
    /// Deletes a product and invalidates related caches.
    /// </summary>
    public async Task DeleteProductAsync(int id)
    {
        _logger.LogInformation("Admin: Deleting product {ProductId}", id);

        var product = await _productRepository.GetAsync(id);
        var category = product.Category;

        await _productRepository.DeleteAsync(id);

        // Invalidate caches
        var cache = _cacheManager.GetCache("ProductCache");
        await cache.RemoveAsync(id.ToString());
        await cache.RemoveAsync($"category:{category}");
        await InvalidateListCachesAsync();

        _logger.LogInformation("Admin: Deleted product {ProductId}, caches invalidated", id);
    }

    /// <summary>
    /// Invalidates all list-based caches.
    /// The Redis backplane ensures API instances receive these invalidations.
    /// </summary>
    private async Task InvalidateListCachesAsync()
    {
        var cache = _cacheManager.GetCache("ProductCache");

        // Clear common list caches
        await cache.RemoveAsync("all-products");
        await cache.RemoveAsync("all-products-active");

        _logger.LogInformation("Admin: Invalidated list caches (synced via backplane)");
    }

    private static ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            TenantId = product.TenantId,
            Name = product.Name,
            Description = product.Description,
            Sku = product.Sku,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            IsActive = product.IsActive,
            Category = product.Category,
            CreationTime = product.CreationTime,
            LastModificationTime = product.LastModificationTime
        };
    }
}
