using Abp.AspNetCore;
using Abp.Castle.Logging.Log4Net;
using Castle.Facilities.Logging;
using CommunityAbp.AspNetZero.FusionCache.DependencyInjection;
using CommunityAbp.AspNetZero.FusionCache.Redis.DependencyInjection;
using ProductCatalog.Admin;
using ProductCatalog.Infrastructure.EntityFramework;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults (health checks, telemetry, service discovery)
builder.AddServiceDefaults();

// Add PostgreSQL with Aspire
builder.AddNpgsqlDbContext<ProductDbContext>("productdb");

// Add Redis with Aspire
builder.AddRedisClient("cache");

// Add MVC
builder.Services.AddControllersWithViews();

// Configure ABP with FusionCache
builder.Services.AddAbpFusionCache(options =>
{
    options.DefaultCacheDuration = TimeSpan.FromMinutes(10);
    options.EnableDistributedCache = true;
    options.EnableBackplane = true; // Critical for cache synchronization
    options.EnableFailSafe = true;
    options.EnableCacheStampedeProtection = true;
    options.EnableMultiTenancy = true;
    options.KeyPrefix = "ProductCatalog";
});

// Configure Redis for FusionCache
builder.Services.AddAbpFusionCacheRedis(builder.Configuration, options =>
{
    // Connection string is auto-injected by Aspire as "cache"
    options.EnableBackplane = true; // Enable backplane for multi-service sync
    options.InstanceName = "ProductCatalog:Admin:";
});

// Add ABP
builder.Services.AddAbp<ProductCatalogAdminModule>(options =>
{
    // Configure ABP Castle Log4Net integration
    options.IocManager.IocContainer.AddFacility<LoggingFacility>(
        f => f.UseAbpLog4Net().WithConfig("log4net.config")
    );
});

var app = builder.Build();

// Configure Aspire endpoints
app.MapDefaultEndpoints();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Initialize ABP
app.UseAbp();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Products}/{action=Index}/{id?}");

// Run database migrations and seed data on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
    await ProductCatalog.Infrastructure.SampleDataSeeder.SeedAsync(dbContext);
}

app.Run();
