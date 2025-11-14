using Abp.AspNetCore;
using Abp.AspNetCore.Configuration;
using Abp.Modules;
using CommunityAbp.AspNetZero.FusionCache;
using CommunityAbp.AspNetZero.FusionCache.Configuration;
using CommunityAbp.AspNetZero.FusionCache.Redis;
using ProductCatalog.Infrastructure;

namespace ProductCatalog.Admin;

/// <summary>
/// Admin module with FusionCache integration for write-heavy operations and cache invalidation.
/// </summary>
[DependsOn(
    typeof(ProductCatalogInfrastructureModule),
    typeof(AbpAspNetCoreModule),
    typeof(AbpFusionCacheModule),
    typeof(AbpFusionCacheRedisModule))]
public class ProductCatalogAdminModule : AbpModule
{
    public override void PreInitialize()
    {
        // Configure ABP
        Configuration.Modules.AbpAspNetCore()
            .CreateControllersForAppServices(
                typeof(ProductCatalogAdminModule).Assembly
            );

        // Configure FusionCache (same config as API for consistency)
        Configuration.Caching.UseFusionCache(options =>
        {
            options.DefaultCacheDuration = TimeSpan.FromMinutes(10);
            options.EnableDistributedCache = true;
            options.EnableBackplane = true; // Critical: enables cache sync across services
            options.EnableFailSafe = true;
            options.EnableCacheStampedeProtection = true;
            options.EnableMultiTenancy = true;
            options.KeyPrefix = "ProductCatalog";
        });
    }

    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(ProductCatalogAdminModule).Assembly);
    }
}
