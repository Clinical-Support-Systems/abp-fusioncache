using Abp.Runtime.Caching.Configuration;
using CommunityAbp.AspNetZero.FusionCache.Configuration;

namespace CommunityAbp.AspNetZero.FusionCache.Redis.Configuration;

/// <summary>
/// Provides extension methods for configuring FusionCache to use Redis as the distributed cache and backplane
/// provider within the ABP framework.
/// </summary>
/// <remarks>These extension methods simplify the integration of FusionCache with Redis by setting the
/// appropriate distributed cache and backplane provider names. Use these methods to enable distributed caching and
/// backplane support with Redis in your ABP application's caching configuration.</remarks>
public static class AbpFusionCacheRedisConfigurationExtensions
{
    /// <summary>
    /// Configures FusionCache to use Redis as the distributed cache and backplane provider within the specified
    /// caching configuration.
    /// </summary>
    /// <remarks>This method enables distributed caching and backplane support using Redis for
    /// FusionCache. It should be called during application startup as part of the caching configuration
    /// setup.</remarks>
    /// <param name="configuration">The caching configuration to modify. This parameter cannot be null.</param>
    /// <param name="optionsAction">An optional action to configure additional Redis-specific options. If null, default options are used.</param>
    public static void UseFusionCacheWithRedis(
        this ICachingConfiguration configuration,
        Action<AbpFusionCacheRedisOptions>? optionsAction = null)
    {
        var redisOptions = new AbpFusionCacheRedisOptions();
        optionsAction?.Invoke(redisOptions);

        configuration.UseFusionCache(fusionOptions =>
        {
            fusionOptions.EnableDistributedCache = true;
            fusionOptions.DistributedCacheProviderName = "Redis";
            fusionOptions.EnableBackplane = redisOptions.EnableBackplane;
            fusionOptions.BackplaneProviderName = "Redis";
        });
    }
}
