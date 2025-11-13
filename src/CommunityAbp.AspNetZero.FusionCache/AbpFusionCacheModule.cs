using System.Reflection;
using Abp;
using Abp.Dependency;
using Abp.Modules;
using Abp.Reflection.Extensions;
using CommunityAbp.AspNetZero.FusionCache.Configuration;
using CommunityAbp.AspNetZero.FusionCache.Internal;
using CommunityAbp.AspNetZero.FusionCache.Providers;
using CommunityAbp.AspNetZero.FusionCache.Runtime.Caching.FusionCache;

namespace CommunityAbp.AspNetZero.FusionCache;

[DependsOn(typeof(AbpKernelModule))]
public class AbpFusionCacheModule : AbpModule
{
    public override void PreInitialize()
    {
        // Register the logger adapter
        IocManager.Register<AbpFusionCacheLogger>(DependencyLifeStyle.Singleton);

        // Register as Microsoft's ILogger for FusionCache
        IocManager.Register<Microsoft.Extensions.Logging.ILogger, AbpFusionCacheLogger>(DependencyLifeStyle.Singleton);

        // Optional: Register the logger factory
        IocManager.Register<Microsoft.Extensions.Logging.ILoggerFactory, AbpFusionCacheLoggerFactory>(DependencyLifeStyle.Singleton);

        IocManager.Register<IAbpFusionCacheProviderRegistry, AbpFusionCacheProviderRegistry>(DependencyLifeStyle.Singleton);
        IocManager.Register<IAbpFusionCacheKeyNormalizer, AbpFusionCacheKeyNormalizer>(DependencyLifeStyle.Singleton);
        IocManager.Register<IAbpFusionCacheSerializer, DefaultAbpFusionCacheSerializer>(DependencyLifeStyle.Transient);
    }

    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(AbpFusionCacheModule).GetAssembly());
    }

    public override void PostInitialize()
    {
        var cacheConfiguration = IocManager.Resolve<IAbpFusionCacheConfiguration>();
        cacheConfiguration.ConfigureFusionCache();
    }
}
