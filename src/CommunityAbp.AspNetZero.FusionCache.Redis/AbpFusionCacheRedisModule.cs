using Abp.Modules;
using Abp.Reflection.Extensions;
using CommunityAbp.AspNetZero.FusionCache.Providers;
using CommunityAbp.AspNetZero.FusionCache.Redis.Providers;

namespace CommunityAbp.AspNetZero.FusionCache.Redis;

/// <summary>
/// Enables Redis-based distributed caching and backplane support for FusionCache in an ABP application.
/// </summary>
/// <remarks>This module depends on AbpFusionCacheModule and registers Redis implementations for
/// distributed cache and backplane providers. To use Redis as the distributed cache or backplane for FusionCache,
/// include this module in your application's module dependencies.</remarks>
[DependsOn(typeof(AbpFusionCacheModule))]
public class AbpFusionCacheRedisModule : AbpModule
{
    public override void PreInitialize()
    {
        var registry = IocManager.Resolve<IAbpFusionCacheProviderRegistry>();
        registry.RegisterDistributedProvider<AbpFusionCacheRedisProvider>();
        registry.RegisterBackplaneProvider<AbpFusionCacheRedisBackplaneProvider>();
    }

    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(AbpFusionCacheRedisModule).GetAssembly());
    }
}
