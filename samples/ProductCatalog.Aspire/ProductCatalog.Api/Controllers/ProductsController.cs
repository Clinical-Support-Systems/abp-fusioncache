using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Api.Services;
using ProductCatalog.Shared.Dtos;

namespace ProductCatalog.Api.Controllers;

/// <summary>
/// Public API for reading product catalog data.
/// All endpoints leverage FusionCache for optimal performance.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        ProductService productService,
        ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all active products.
    /// </summary>
    /// <remarks>
    /// This endpoint demonstrates:
    /// - L1 (memory) + L2 (Redis) hybrid caching
    /// - Automatic tenant isolation
    /// - Fail-safe mode (serves stale data if Redis is down)
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(List<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true)
    {
        _logger.LogInformation("GET /api/products - activeOnly={ActiveOnly}", activeOnly);

        var products = await _productService.GetAllProductsAsync(activeOnly);
        return Ok(products);
    }

    /// <summary>
    /// Gets a single product by ID.
    /// </summary>
    /// <remarks>
    /// This endpoint demonstrates:
    /// - Cache stampede protection (concurrent requests for same ID)
    /// - Tenant-aware cache keys
    /// - Fast L1 memory cache hits
    /// </remarks>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(int id)
    {
        _logger.LogInformation("GET /api/products/{Id}", id);

        var product = await _productService.GetProductAsync(id);

        if (product == null)
        {
            return NotFound(new { message = $"Product with ID {id} not found" });
        }

        return Ok(product);
    }

    /// <summary>
    /// Gets products by category.
    /// </summary>
    [HttpGet("category/{category}")]
    [ProducesResponseType(typeof(List<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCategory(string category)
    {
        _logger.LogInformation("GET /api/products/category/{Category}", category);

        var products = await _productService.GetProductsByCategoryAsync(category);
        return Ok(products);
    }

    /// <summary>
    /// Searches products by name.
    /// </summary>
    /// <remarks>
    /// This endpoint does NOT use caching due to high variability of search terms.
    /// </remarks>
    [HttpGet("search")]
    [ProducesResponseType(typeof(List<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return BadRequest(new { message = "Search term is required" });
        }

        _logger.LogInformation("GET /api/products/search?q={Query}", q);

        var products = await _productService.SearchProductsAsync(q);
        return Ok(products);
    }

    /// <summary>
    /// Health check endpoint.
    /// </summary>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            service = "ProductCatalog.Api"
        });
    }
}
