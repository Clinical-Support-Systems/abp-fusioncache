using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Admin.Services;
using ProductCatalog.Shared.Dtos;

namespace ProductCatalog.Admin.Controllers;

/// <summary>
/// Admin controller for managing products.
/// Demonstrates cache invalidation with Redis backplane synchronization.
/// </summary>
public class ProductsController : Controller
{
    private readonly ProductAdminService _productAdminService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        ProductAdminService productAdminService,
        ILogger<ProductsController> logger)
    {
        _productAdminService = productAdminService;
        _logger = logger;
    }

    /// <summary>
    /// Lists all products (admin view).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        _logger.LogInformation("Admin: Listing all products");

        var products = await _productAdminService.GetAllProductsAsync();
        return View(products);
    }

    /// <summary>
    /// Shows form to create a new product.
    /// </summary>
    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateUpdateProductDto());
    }

    /// <summary>
    /// Creates a new product and invalidates cache.
    /// </summary>
    /// <remarks>
    /// This demonstrates:
    /// - Cache invalidation via _cacheManager.RemoveAsync()
    /// - Redis backplane broadcasting invalidation to API instances
    /// - Multi-service cache synchronization
    /// </remarks>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUpdateProductDto model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        _logger.LogInformation("Admin: Creating product {Name}", model.Name);

        await _productAdminService.CreateProductAsync(model);

        TempData["SuccessMessage"] = $"Product '{model.Name}' created successfully. Cache invalidated across all instances.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Shows form to edit an existing product.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _productAdminService.GetProductAsync(id);

        if (product == null)
        {
            return NotFound();
        }

        var model = new CreateUpdateProductDto
        {
            Name = product.Name,
            Description = product.Description,
            Sku = product.Sku,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            IsActive = product.IsActive,
            Category = product.Category
        };

        ViewBag.ProductId = id;
        return View(model);
    }

    /// <summary>
    /// Updates an existing product and invalidates cache.
    /// </summary>
    /// <remarks>
    /// After update:
    /// 1. Cache is invalidated locally
    /// 2. Redis backplane broadcasts to API instances
    /// 3. API instances auto-invalidate their L1 memory cache
    /// 4. Next API request fetches fresh data from database
    /// </remarks>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CreateUpdateProductDto model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ProductId = id;
            return View(model);
        }

        _logger.LogInformation("Admin: Updating product {ProductId}", id);

        await _productAdminService.UpdateProductAsync(id, model);

        TempData["SuccessMessage"] = $"Product '{model.Name}' updated successfully. Cache synced via backplane.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Deletes a product and invalidates cache.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogInformation("Admin: Deleting product {ProductId}", id);

        await _productAdminService.DeleteProductAsync(id);

        TempData["SuccessMessage"] = "Product deleted successfully. Cache invalidated.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Health check endpoint.
    /// </summary>
    [HttpGet]
    public IActionResult Health()
    {
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            service = "ProductCatalog.Admin"
        });
    }
}
