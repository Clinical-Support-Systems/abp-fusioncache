using Abp.AspNetCore;
using Abp.AspNetCore.Configuration;
using Abp.Modules;
using CommunityAbp.AspNetZero.FusionCache;
using CommunityAbp.AspNetZero.FusionCache.Configuration;
using CommunityAbp.AspNetZero.FusionCache.Redis;
using ProductCatalog.Infrastructure;

namespace ProductCatalog.Api;

/// <summary>
/// API module with FusionCache integration for read-heavy operations.
/// </summary>
[DependsOn(
    typeof(ProductCatalogInfrastructureModule),
    typeof(AbpAspNetCoreModule),
    typeof(AbpFusionCacheModule),
    typeof(AbpFusionCacheRedisModule))]
public class ProductCatalogApiModule : AbpModule
{
    public override void PreInitialize()
    {
        // Configure ABP
        Configuration.Modules.AbpAspNetCore()
            .CreateControllersForAppServices(
                typeof(ProductCatalogApiModule).Assembly
            );

        // Configure FusionCache with all features enabled
        Configuration.Caching.UseFusionCache(options =>
        {
            options.DefaultCacheDuration = TimeSpan.FromMinutes(10);
            options.EnableDistributedCache = true;
            options.EnableBackplane = true;
            options.EnableFailSafe = true;
            options.EnableCacheStampedeProtection = true;
            options.EnableMultiTenancy = true;
            options.EnableEagerRefresh = false; // Can be enabled for critical data
            options.EagerRefreshThreshold = 0.9f; // Refresh at 90% of lifetime
            options.KeyPrefix = "ProductCatalog";
        });
    }

    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(ProductCatalogApiModule).Assembly);
    }
}
