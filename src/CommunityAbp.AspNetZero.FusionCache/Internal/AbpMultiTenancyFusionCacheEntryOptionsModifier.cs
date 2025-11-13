using Abp.Configuration.Startup;
using Abp.Dependency;
using Abp.Runtime.Session;
using CommunityAbp.AspNetZero.FusionCache.Options;
using Microsoft.Extensions.Caching.Memory;
using ZiggyCreatures.Caching.Fusion;

namespace CommunityAbp.AspNetZero.FusionCache.Internal
{
    /// <summary>
    /// Modifies FusionCache entry options based on multi-tenancy context.
    /// </summary>
    public interface IAbpMultiTenancyFusionCacheEntryOptionsModifier
    {
        /// <summary>
        /// Modifies the provided FusionCache entry options based on the current tenant context.
        /// </summary>
        /// <param name="options">The original FusionCache entry options to modify.</param>
        /// <param name="cacheKey">The cache key being used (after normalization).</param>
        /// <returns>Modified FusionCache entry options appropriate for the current tenant context.</returns>
        FusionCacheEntryOptions ModifyOptions(FusionCacheEntryOptions options, string cacheKey);
    }

    /// <summary>
    /// Modifies FusionCache entry options based on multi-tenancy context to provide
    /// tenant-appropriate caching behavior.
    /// </summary>
    public class AbpMultiTenancyFusionCacheEntryOptionsModifier
        : IAbpMultiTenancyFusionCacheEntryOptionsModifier, ITransientDependency
    {
        private readonly IMultiTenancyConfig _multiTenancyConfig;
        private readonly IAbpSession _session;
        private readonly AbpFusionCacheOptions _cacheOptions;

        /// <summary>
        /// Initializes a new instance of the AbpMultiTenancyFusionCacheEntryOptionsModifier.
        /// </summary>
        public AbpMultiTenancyFusionCacheEntryOptionsModifier(
            IMultiTenancyConfig multiTenancyConfig,
            IAbpSession session,
            AbpFusionCacheOptions cacheOptions)
        {
            _multiTenancyConfig = multiTenancyConfig;
            _session = session;
            _cacheOptions = cacheOptions;
        }

        /// <summary>
        /// Modifies FusionCache entry options based on the current tenant context.
        /// </summary>
        public FusionCacheEntryOptions ModifyOptions(FusionCacheEntryOptions options, string cacheKey)
        {
            // If multi-tenancy is disabled, return original options
            if (!_cacheOptions.EnableMultiTenancy || !_multiTenancyConfig.IsEnabled)
            {
                return options;
            }

            // Create a copy to avoid modifying the original
            var modifiedOptions = options.Duplicate();

            // Apply tenant-specific modifications
            if (_session.TenantId.HasValue)
            {
                ApplyTenantSpecificOptions(modifiedOptions, _session.TenantId.Value, cacheKey);
            }
            else
            {
                ApplyHostOptions(modifiedOptions, cacheKey);
            }

            return modifiedOptions;
        }

        /// <summary>
        /// Applies cache options specific to tenant data.
        /// </summary>
        private void ApplyTenantSpecificOptions(FusionCacheEntryOptions options, int tenantId, string cacheKey)
        {
            // Tenant-specific data might need different cache behavior

            // 1. Shorter cache duration for tenant-specific data to allow for customizations
            var tenantCacheDuration = CalculateTenantCacheDuration(options.Duration);
            options.SetDuration(tenantCacheDuration);

            // 2. Enable fail-safe for tenant data to improve resilience in multi-tenant scenarios
            if (!options.IsFailSafeEnabled)
            {
                var failSafeDuration = TimeSpan.FromHours(1); // Reasonable fail-safe window
                options.SetFailSafe(true, failSafeDuration);
            }

            // 3. Tenant data might benefit from eager refresh to keep data fresh
            if (_cacheOptions.EnableEagerRefresh)
            {
                options.SetEagerRefresh(_cacheOptions.EagerRefreshThreshold);
            }

            // 4. Higher priority for tenant data since it's user-specific
            options.Priority = CacheItemPriority.High;

            // 5. Add small jitter to prevent cache stampede in multi-tenant scenarios
            if (_cacheOptions.EnableCacheStampedeProtection)
            {
                // Small jitter based on tenant ID for distribution
                var jitterMs = (tenantId % 100) + 50; // 50-149ms range
                options.JitterMaxDuration = TimeSpan.FromMilliseconds(jitterMs);
            }
        }

        /// <summary>
        /// Applies cache options specific to host/shared data.
        /// </summary>
        private void ApplyHostOptions(FusionCacheEntryOptions options, string cacheKey)
        {
            // Host data is shared across tenants and can be cached longer

            // 1. Longer cache duration for shared/host data
            var hostCacheDuration = CalculateHostCacheDuration(options.Duration);
            options.SetDuration(hostCacheDuration);

            // 2. Host data usually has stronger fail-safe requirements
            if (!options.IsFailSafeEnabled)
            {
                var failSafeDuration = TimeSpan.FromHours(4); // Longer fail-safe for shared data
                options.SetFailSafe(true, failSafeDuration);
            }

            // 3. Normal priority for host data since it's shared
            options.Priority = CacheItemPriority.Normal;

            // 4. Less aggressive eager refresh for shared data
            if (_cacheOptions.EnableEagerRefresh)
            {
                var adjustedThreshold = Math.Min(_cacheOptions.EagerRefreshThreshold + 0.1f, 0.95f);
                options.SetEagerRefresh(adjustedThreshold);
            }

            // 5. Minimal jitter for host data to maintain consistency
            if (_cacheOptions.EnableCacheStampedeProtection)
            {
                options.JitterMaxDuration = TimeSpan.FromMilliseconds(25); // Small fixed jitter
            }
        }

        /// <summary>
        /// Calculates appropriate cache duration for tenant-specific data.
        /// </summary>
        private TimeSpan CalculateTenantCacheDuration(TimeSpan originalDuration)
        {
            // Tenant data might change more frequently due to customizations
            // Reduce cache duration by 25% to ensure fresher data
            var adjustedTicks = originalDuration.Ticks * 75 / 100;
            return new TimeSpan(adjustedTicks);
        }

        /// <summary>
        /// Calculates appropriate cache duration for host/shared data.
        /// </summary>
        private TimeSpan CalculateHostCacheDuration(TimeSpan originalDuration)
        {
            // Host data changes less frequently and can be cached longer
            // Increase cache duration by 50% for better performance
            var adjustedTicks = originalDuration.Ticks * 150 / 100;
            return new TimeSpan(adjustedTicks);
        }
    }
}
