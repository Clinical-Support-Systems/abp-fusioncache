using Abp.Dependency;
using Abp.Runtime.Caching;
using Abp.Runtime.Caching.Configuration;
using CommunityAbp.AspNetZero.FusionCache.Options;
using CommunityAbp.AspNetZero.FusionCache.Runtime.Caching.FusionCache;

namespace CommunityAbp.AspNetZero.FusionCache.Configuration
{
    /// <summary>
    /// Provides extension methods to configure FusionCache as the caching provider within the ABP framework.
    /// </summary>
    /// <remarks>These extension methods allow integration of FusionCache with ABP's caching system by
    /// registering the necessary services and enabling optional configuration of FusionCache options. Use these methods
    /// during application startup to replace the default cache manager with FusionCache.</remarks>
    public static class AbpFusionCacheConfigurationExtensions
    {
        /// <summary>
        /// Enables FusionCache as the caching provider using the default configuration.
        /// </summary>
        /// <remarks>This method registers FusionCache with default options. To customize FusionCache
        /// behavior, use the overload that accepts a configuration action.</remarks>
        /// <param name="cachingConfiguration">The caching configuration to which FusionCache will be added. Cannot be null.</param>
        public static void UseFusionCache(this ICachingConfiguration cachingConfiguration)
        {
            cachingConfiguration.UseFusionCache(options => { });
        }

        /// <summary>
        /// Enables FusionCache as the caching provider for the specified caching configuration, allowing for advanced
        /// caching features and integration.
        /// </summary>
        /// <remarks>Call this method during application startup to register FusionCache as the cache
        /// manager. This method registers required FusionCache services as singletons and allows for custom
        /// configuration via the options action.</remarks>
        /// <param name="cachingConfiguration">The caching configuration to which FusionCache will be added. Cannot be null.</param>
        /// <param name="optionsAction">An optional action to configure FusionCache-specific options. If null, default options are used.</param>
        public static void UseFusionCache(this ICachingConfiguration cachingConfiguration, Action<AbpFusionCacheOptions>? optionsAction = null)
        {
            var iocManager = cachingConfiguration.AbpConfiguration.IocManager;

            iocManager.RegisterIfNot<IAbpFusionCacheConfiguration, AbpFusionCacheConfiguration>(DependencyLifeStyle.Singleton);
            iocManager.RegisterIfNot<ICacheManager, AbpFusionCacheManager>();

            if (optionsAction != null)
            {
                var fusionCacheConfig = iocManager.Resolve<IAbpFusionCacheConfiguration>();

                optionsAction(fusionCacheConfig.Options);
            }
        }
    }
}
