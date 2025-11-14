using Abp.AspNetCore;
using Abp.Castle.Logging.Log4Net;
using Castle.Facilities.Logging;
using CommunityAbp.AspNetZero.FusionCache.DependencyInjection;
using CommunityAbp.AspNetZero.FusionCache.Redis.DependencyInjection;
using ProductCatalog.Api;
using ProductCatalog.Infrastructure.EntityFramework;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults (health checks, telemetry, service discovery)
builder.AddServiceDefaults();

// Add PostgreSQL with Aspire
builder.AddNpgsqlDbContext<ProductDbContext>("productdb");

// Add Redis with Aspire
builder.AddRedisClient("cache");

// Add controllers and API documentation
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "ProductCatalog API",
        Version = "v1",
        Description = "Public API demonstrating ABP FusionCache with .NET Aspire"
    });
});

//// Configure ABP with FusionCache
//builder.Services.AddAbpFusionCache(options =>
//{
//    options.DefaultCacheDuration = TimeSpan.FromMinutes(10);
//    options.EnableDistributedCache = true;
//    options.EnableBackplane = true;
//    options.EnableFailSafe = true;
//    options.EnableCacheStampedeProtection = true;
//    options.EnableMultiTenancy = true;
//    options.KeyPrefix = "ProductCatalog";
//});

// Configure Redis for FusionCache
builder.Services.AddAbpFusionCacheRedis(builder.Configuration, options =>
{
    // Connection string is auto-injected by Aspire as "cache"
    options.EnableBackplane = true;
    options.InstanceName = "ProductCatalog:Api:";
});

// Add ABP
builder.Services.AddAbp<ProductCatalogApiModule>(options =>
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
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "ProductCatalog API v1");
        options.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });
}

app.UseHttpsRedirection();

// Initialize ABP
app.UseAbp();

app.UseAuthorization();

app.MapControllers();

// Run database migrations and seed data on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
    await ProductCatalog.Infrastructure.SampleDataSeeder.SeedAsync(dbContext);
}

app.Run();
