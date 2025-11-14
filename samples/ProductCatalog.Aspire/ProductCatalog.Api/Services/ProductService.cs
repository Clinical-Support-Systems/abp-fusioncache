using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Runtime.Caching;
using Abp.Runtime.Session;
using ProductCatalog.Shared.Dtos;
using ProductCatalog.Shared.Entities;

namespace ProductCatalog.Api.Services;

/// <summary>
/// Service for reading product data with aggressive caching.
/// Demonstrates FusionCache in action for read-heavy operations.
/// </summary>
public class ProductService : ITransientDependency
{
    private readonly ICacheManager _cacheManager;
    private readonly IRepository<Product, int> _productRepository;
    private readonly IAbpSession _session;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        ICacheManager cacheManager,
        IRepository<Product, int> productRepository,
        IAbpSession abpSession,
        ILogger<ProductService> logger)
    {
        _cacheManager = cacheManager;
        _productRepository = productRepository;
        _session = abpSession;
        _logger = logger;
    }

    /// <summary>
    /// Gets a single product by ID with caching.
    /// Cache key is tenant-aware automatically.
    /// </summary>
    public async Task<ProductDto?> GetProductAsync(int productId)
    {
        var cache = _cacheManager.GetCache("ProductCache");

        _logger.LogInformation(
            "Getting product {ProductId} for tenant {TenantId}",
            productId,
            _session.TenantId);

        var product = await cache.GetAsync(
            productId.ToString(),
            async () =>
            {
                _logger.LogInformation("Cache MISS for product {ProductId} - fetching from database", productId);

                var entity = await _productRepository.FirstOrDefaultAsync(p => p.Id == productId);
                if (entity == null)
                {
                    return null;
                }

                return MapToDto(entity);
            }
        );

        if (product != null)
        {
            _logger.LogInformation("Cache HIT for product {ProductId}", productId);
        }

        return product;
    }

    /// <summary>
    /// Gets all active products with caching.
    /// Uses shorter cache duration for list operations.
    /// </summary>
    public async Task<List<ProductDto>> GetAllProductsAsync(bool activeOnly = true)
    {
        var cache = _cacheManager.GetCache("ProductCache");
        var cacheKey = activeOnly ? "all-products-active" : "all-products";

        _logger.LogInformation(
            "Getting all products (activeOnly={ActiveOnly}) for tenant {TenantId}",
            activeOnly,
            _session.TenantId);

        var products = await cache.GetAsync(
            cacheKey,
            async () =>
            {
                _logger.LogInformation("Cache MISS for {CacheKey} - fetching from database", cacheKey);

                var query = _productRepository.GetAll();

                if (activeOnly)
                {
                    query = query.Where(p => p.IsActive);
                }

                var entities = await _productRepository.GetAllListAsync(query);
                return entities.Select(MapToDto).ToList();
            },
            slidingExpireTime: TimeSpan.FromMinutes(5) // Shorter cache for lists
        );

        _logger.LogInformation("Returning {Count} products from cache", products.Count);
        return products;
    }

    /// <summary>
    /// Gets products by category with caching.
    /// </summary>
    public async Task<List<ProductDto>> GetProductsByCategoryAsync(string category)
    {
        var cache = _cacheManager.GetCache("ProductCache");
        var cacheKey = $"category:{category}";

        _logger.LogInformation(
            "Getting products for category {Category} and tenant {TenantId}",
            category,
            _session.TenantId);

        return await cache.GetAsync(
            cacheKey,
            async () =>
            {
                _logger.LogInformation("Cache MISS for category {Category}", category);

                var entities = await _productRepository.GetAllListAsync(
                    p => p.Category == category && p.IsActive
                );

                return entities.Select(MapToDto).ToList();
            },
            slidingExpireTime: TimeSpan.FromMinutes(5)
        );
    }

    /// <summary>
    /// Searches products by name with caching.
    /// </summary>
    public async Task<List<ProductDto>> SearchProductsAsync(string searchTerm)
    {
        // Don't cache search results as they can be highly variable
        _logger.LogInformation("Searching products for term: {SearchTerm}", searchTerm);

        var entities = await _productRepository.GetAllListAsync(
            p => p.Name.Contains(searchTerm) && p.IsActive
        );

        return entities.Select(MapToDto).ToList();
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
