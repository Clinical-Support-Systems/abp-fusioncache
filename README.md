# ABP FusionCache Integration

[![NuGet](https://img.shields.io/nuget/v/CommunityAbp.AspNetZero.FusionCache.svg)](https://www.nuget.org/packages/CommunityAbp.AspNetZero.FusionCache/)
[![License](https://img.shields.io/github/license/Clinical-Support-Systems/abp-fusioncache)](LICENSE)

This package integrates [FusionCache](https://github.com/ZiggyCreatures/FusionCache) into ASP.NET Zero and ABP Framework applications, providing a drop-in replacement for the standard Redis cache with advanced resiliency features and hybrid caching capabilities.

## Features

- **Hybrid Caching**: Combines L1 (in-memory) and L2 (distributed) caching layers for optimal performance
- **Fail-Safe Mode**: Automatically serves stale data when the cache backend is unavailable
- **Cache Stampede Protection**: Prevents multiple concurrent requests from overwhelming your data source
- **Multi-Tenancy Support**: Tenant-aware cache key normalization and isolation
- **Redis Backplane**: Synchronizes cache across multiple application instances
- **Eager Refresh**: Proactively refreshes cache entries before expiration
- **Easy Configuration**: Fluent API for simple setup and customization

## Installation

### Basic Installation (In-Memory Only)

Install the core package:

```bash
dotnet add package CommunityAbp.AspNetZero.FusionCache
```

### Redis Integration

For distributed caching with Redis, install the Redis package:

```bash
dotnet add package CommunityAbp.AspNetZero.FusionCache.Redis
```

## Quick Start

### ASP.NET Core / ABP vNext

#### 1. Basic Configuration (In-Memory Only)

```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Add FusionCache with default options
        services.AddAbpFusionCache(options =>
        {
            options.DefaultCacheDuration = TimeSpan.FromMinutes(30);
            options.EnableFailSafe = true;
            options.EnableCacheStampedeProtection = true;
        });
    }
}
```

#### 2. With Redis Distributed Cache

```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Add FusionCache
        services.AddAbpFusionCache(options =>
        {
            options.EnableDistributedCache = true;
            options.EnableBackplane = true;
        });

        // Add Redis integration
        services.AddAbpFusionCacheRedis(Configuration, options =>
        {
            options.ConnectionString = "localhost:6379";
            options.EnableBackplane = true;
            options.InstanceName = "MyApp:";
        });
    }
}
```

### ABP Framework (Classic)

Configure FusionCache in your module's `PreInitialize` method:

```csharp
[DependsOn(typeof(AbpKernelModule))]
public class MyApplicationModule : AbpModule
{
    public override void PreInitialize()
    {
        // Enable FusionCache
        Configuration.Caching.UseFusionCache(options =>
        {
            options.DefaultCacheDuration = TimeSpan.FromMinutes(30);
            options.EnableFailSafe = true;
            options.EnableMultiTenancy = true;
        });
    }
}
```

## Configuration Options

### Core Options (AbpFusionCacheOptions)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `DefaultCacheDuration` | `TimeSpan` | 30 minutes | Default duration for cached items |
| `EnableMemoryCache` | `bool` | `true` | Enable L1 in-memory caching |
| `EnableDistributedCache` | `bool` | `false` | Enable L2 distributed caching |
| `EnableFailSafe` | `bool` | `true` | Serve stale data when cache backend is unavailable |
| `EnableCacheStampedeProtection` | `bool` | `true` | Prevent cache stampede scenarios |
| `EnableEagerRefresh` | `bool` | `false` | Proactively refresh cache before expiration |
| `EagerRefreshThreshold` | `float` | `0.9f` | Threshold (0-1) for eager refresh trigger |
| `EnableMultiTenancy` | `bool` | `true` | Enable tenant-aware caching |
| `KeyPrefix` | `string` | `"AbpCache"` | Prefix for all cache keys |
| `EnableBackplane` | `bool` | `false` | Enable distributed cache synchronization |

### Redis Options (AbpFusionCacheRedisOptions)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ConnectionString` | `string` | `null` | Redis connection string |
| `Database` | `int` | `0` | Redis database index |
| `EnableBackplane` | `bool` | `false` | Enable Redis backplane for cache sync |
| `BackplaneChannelPrefix` | `string` | `"AbpFusionCache"` | Prefix for backplane channels |
| `ConnectTimeout` | `TimeSpan` | 5 seconds | Connection timeout |
| `SyncTimeout` | `TimeSpan` | 5 seconds | Synchronous operation timeout |
| `InstanceName` | `string` | `"AbpFusionCache"` | Cache instance name |

## Usage Examples

### Basic Cache Operations

```csharp
public class ProductService
{
    private readonly ICacheManager _cacheManager;

    public ProductService(ICacheManager cacheManager)
    {
        _cacheManager = cacheManager;
    }

    public async Task<Product> GetProductAsync(int productId)
    {
        var cache = _cacheManager.GetCache("ProductCache");
        
        return await cache.GetAsync(
            productId.ToString(),
            async () => await LoadProductFromDatabase(productId)
        );
    }

    private async Task<Product> LoadProductFromDatabase(int productId)
    {
        // Your data loading logic
    }
}
```

### Custom Cache Duration

```csharp
public async Task<Product> GetProductAsync(int productId)
{
    var cache = _cacheManager.GetCache("ProductCache");
    
    return await cache.GetAsync(
        productId.ToString(),
        async () => await LoadProductFromDatabase(productId),
        slidingExpireTime: TimeSpan.FromMinutes(10)
    );
}
```

### Multi-Tenancy Support

The cache automatically handles tenant isolation:

```csharp
// Cache keys are automatically prefixed with tenant ID
var tenantProduct = await cache.GetAsync(
    "product:123",
    async () => await GetTenantProduct(123)
);

// Different tenants get different cache entries for the same key
```

## Connection String Configuration

The Redis integration looks for connection strings in this order:

1. `AbpFusionCacheRedisOptions.ConnectionString`
2. `ConnectionStrings:Redis` in `appsettings.json`
3. `Redis:Configuration` in `appsettings.json`

### Example appsettings.json

```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=MyApp;...",
    "Redis": "localhost:6379,password=mypassword"
  },
  "Redis": {
    "Configuration": "localhost:6379"
  }
}
```

## Advanced Scenarios

### Custom Cache Configuration per Cache Name

```csharp
Configuration.Caching.Configure("UserCache", cache =>
{
    cache.DefaultSlidingExpireTime = TimeSpan.FromHours(1);
});

Configuration.Caching.Configure("ProductCache", cache =>
{
    cache.DefaultSlidingExpireTime = TimeSpan.FromMinutes(15);
});
```

### Fail-Safe with Custom Timeouts

```csharp
services.AddAbpFusionCache(options =>
{
    options.EnableFailSafe = true;
    options.DefaultCacheDuration = TimeSpan.FromMinutes(30);
    // Stale data will be served for up to 1 hour if backend is down
});
```

### High-Availability Setup with Redis Backplane

```csharp
services.AddAbpFusionCache(options =>
{
    options.EnableDistributedCache = true;
    options.EnableBackplane = true;
    options.EnableFailSafe = true;
});

services.AddAbpFusionCacheRedis(Configuration, options =>
{
    options.ConnectionString = "redis-cluster:6379,redis-cluster:6380";
    options.EnableBackplane = true;
    options.BackplaneChannelPrefix = "MyApp:Cache:";
});
```

### Eager Refresh for Critical Data

```csharp
services.AddAbpFusionCache(options =>
{
    options.EnableEagerRefresh = true;
    options.EagerRefreshThreshold = 0.9f; // Refresh at 90% of lifetime
});
```

## Multi-Tenancy Behavior

When multi-tenancy is enabled, the cache automatically:

- **Tenant-specific data**: Shorter cache duration (75% of default) for fresher data
- **Host data**: Longer cache duration (150% of default) for shared data
- **Automatic key isolation**: Tenant ID is automatically included in cache keys
- **Fail-safe tuning**: Different fail-safe windows for tenant vs. host data

## Performance Considerations

### Memory vs. Distributed Cache

- **L1 (Memory Cache)**: Ultra-fast, but per-instance (not shared)
- **L2 (Distributed Cache)**: Slower, but shared across instances
- **Hybrid Mode**: Best of both worlds - fast reads with shared data

### Recommended Settings

**Single Instance (Development)**
```csharp
options.EnableMemoryCache = true;
options.EnableDistributedCache = false;
```

**Multi-Instance (Production)**
```csharp
options.EnableMemoryCache = true;
options.EnableDistributedCache = true;
options.EnableBackplane = true;
```

## Troubleshooting

### Cache Not Working

1. Verify FusionCache is registered: Check that `AddAbpFusionCache()` is called
2. Check Redis connection: Ensure Redis is running and connection string is correct
3. Review logs: FusionCache logs cache operations at various levels

### Stale Data Issues

- Ensure backplane is enabled if running multiple instances
- Verify backplane channel prefix is consistent across instances
- Check network connectivity between instances and Redis

### High Memory Usage

- Reduce `DefaultCacheDuration` to expire items sooner
- Disable `EnableMemoryCache` if distributed cache is sufficient
- Implement cache eviction policies using ASP.NET Core's `MemoryCacheOptions`

## Dependencies

- **ABP Framework**: 10.3.x or higher
- **FusionCache**: 2.4.x or higher
- **.NET**: 8.0 or 9.0
- **Redis** (optional): Any version supported by StackExchange.Redis

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- [FusionCache](https://github.com/ZiggyCreatures/FusionCache) by Jody Donetti
- [ABP Framework](https://abp.io/) by Volosoft
- [ASP.NET Zero](https://aspnetzero.com/) by Volosoft

## Support

For issues, questions, or contributions, please visit:
- GitHub Issues: https://github.com/Clinical-Support-Systems/abp-fusioncache/issues
- FusionCache Documentation: https://github.com/ZiggyCreatures/FusionCache
- ABP Documentation: https://docs.abp.io/
