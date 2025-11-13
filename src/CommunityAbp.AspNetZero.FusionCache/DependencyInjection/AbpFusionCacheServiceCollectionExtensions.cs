using CommunityAbp.AspNetZero.FusionCache.Internal;
using CommunityAbp.AspNetZero.FusionCache.Options;
using CommunityAbp.AspNetZero.FusionCache.Providers;
using CommunityAbp.AspNetZero.FusionCache.Providers.Null;
using CommunityAbp.AspNetZero.FusionCache.Runtime.Caching.FusionCache;
using Microsoft.Extensions.DependencyInjection;
using ZiggyCreatures.Caching.Fusion;

namespace CommunityAbp.AspNetZero.FusionCache.DependencyInjection
{
    public static class AbpFusionCacheServiceCollectionExtensions
    {
        /// <summary>
        /// Adds FusionCache services to the service collection with default configuration.
        /// </summary>
        public static IServiceCollection AddAbpFusionCache(
            this IServiceCollection services,
            Action<AbpFusionCacheOptions> optionsAction = null)
        {
            // Register ABP FusionCache options
            var options = new AbpFusionCacheOptions();
            optionsAction?.Invoke(options);
            services.AddSingleton(options);

            services.AddSingleton<Microsoft.Extensions.Logging.ILogger, AbpFusionCacheLogger>();
            services.AddSingleton<Microsoft.Extensions.Logging.ILoggerFactory, AbpFusionCacheLoggerFactory>();

            // Register core FusionCache
            services.AddFusionCache().WithOptions(fusionOptions =>
            {
                var opt = new FusionCacheEntryOptions()
                    .SetDuration(options.DefaultCacheDuration)
                    .SetFailSafe(options.EnableFailSafe);

                if (options.EnableCacheStampedeProtection)
                {
                    opt.JitterMaxDuration = TimeSpan.FromMilliseconds(30);
                }

                fusionOptions.DefaultEntryOptions = opt;
            });

            // Register ABP-specific services
            services.AddSingleton<IAbpFusionCacheKeyNormalizer, AbpFusionCacheKeyNormalizer>();
            services.AddTransient<IAbpFusionCacheSerializer, DefaultAbpFusionCacheSerializer>();
            services.AddTransient<IAbpMultiTenancyFusionCacheEntryOptionsModifier, AbpMultiTenancyFusionCacheEntryOptionsModifier>();

            // Register provider registry
            services.AddSingleton<IAbpFusionCacheProviderRegistry, AbpFusionCacheProviderRegistry>();

            // Register null providers as fallbacks
            services.AddSingleton<NullDistributedCacheProvider>();
            services.AddSingleton<NullBackplaneProvider>();

            // Register memory cache if not already registered
            services.AddMemoryCache();

            // Add JSON serializer if distributed cache is enabled
            if (options.EnableDistributedCache)
            {
                services.AddFusionCacheSystemTextJsonSerializer();
            }

            return services;
        }

        /// <summary>
        /// Configures FusionCache to use an existing distributed cache provider.
        /// </summary>
        public static IServiceCollection WithDistributedCache(
            this IServiceCollection services,
            string providerName,
            Action<object>? configureProvider = null)
        {
            services.Configure<AbpFusionCacheOptions>(options =>
            {
                options.EnableDistributedCache = true;
                options.DistributedCacheProviderName = providerName;
            });

            return services;
        }

        /// <summary>
        /// Configures FusionCache to use a backplane provider.
        /// </summary>
        public static IServiceCollection WithBackplane(
            this IServiceCollection services,
            string providerName,
            Action<object>? configureProvider = null)
        {
            services.Configure<AbpFusionCacheOptions>(options =>
            {
                options.EnableBackplane = true;
                options.BackplaneProviderName = providerName;
            });

            return services;
        }
    }
}
