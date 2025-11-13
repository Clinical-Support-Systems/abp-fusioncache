using Abp.Configuration.Startup;
using Abp.Dependency;
using Abp.Runtime.Session;
using CommunityAbp.AspNetZero.FusionCache.Options;

namespace CommunityAbp.AspNetZero.FusionCache.Runtime.Caching.FusionCache
{
    /// <summary>
    /// Provides functionality to normalize cache keys for use with FusionCache, applying global prefixes and tenant
    /// isolation as configured.
    /// </summary>
    /// <remarks>This class ensures that cache keys are consistently formatted according to application-wide
    /// settings and multi-tenancy requirements. It is typically used internally by caching infrastructure to prevent
    /// key collisions and to support tenant-specific cache segregation.</remarks>
    public class AbpFusionCacheKeyNormalizer : IAbpFusionCacheKeyNormalizer, ISingletonDependency
    {
        private readonly IMultiTenancyConfig _multiTenancyConfig;
        private readonly IAbpSession _session;
        private readonly AbpFusionCacheOptions _options;

        /// <summary>
        /// Initializes a new instance of the AbpFusionCacheKeyNormalizer class with the specified multi-tenancy
        /// configuration, session, and cache options.
        /// </summary>
        /// <param name="multiTenancyConfig">The multi-tenancy configuration used to determine tenant-specific cache key normalization behavior. Cannot
        /// be null.</param>
        /// <param name="session">The current session information, used to identify the active tenant and user context. Cannot be null.</param>
        /// <param name="options">The options that configure cache key normalization behavior. Cannot be null.</param>
        public AbpFusionCacheKeyNormalizer(
            IMultiTenancyConfig multiTenancyConfig,
            IAbpSession session,
            AbpFusionCacheOptions options)
        {
            _multiTenancyConfig = multiTenancyConfig;
            _session = session;
            _options = options;
        }

        /// <summary>
        /// Generates a normalized cache key by applying the configured key prefix and tenant isolation settings.
        /// </summary>
        /// <remarks>If multi-tenancy is enabled, the normalized key will include the current tenant
        /// identifier to ensure tenant isolation. If a key prefix is configured, it will be prepended to the key. These
        /// behaviors help prevent key collisions across tenants and application domains.</remarks>
        /// <param name="key">The original cache key to normalize. Cannot be null or empty.</param>
        /// <returns>A normalized cache key string that includes any configured key prefix and tenant identifier, if applicable.</returns>
        /// <exception cref="ArgumentException">Thrown if the specified <paramref name="key"/> is null or empty.</exception>
        public string NormalizeKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Cache key cannot be null or empty", nameof(key));
            }

            var normalizedKey = key;

            // Add global key prefix if configured
            if (!string.IsNullOrEmpty(_options.KeyPrefix))
            {
                normalizedKey = $"{_options.KeyPrefix}:{normalizedKey}";
            }

            // Add tenant isolation if multi-tenancy is enabled
            if (_options.EnableMultiTenancy && _multiTenancyConfig.IsEnabled)
            {
                var tenantId = _session.TenantId?.ToString() ?? "host";
                normalizedKey = $"tenant:{tenantId}:{normalizedKey}";
            }

            return normalizedKey;
        }
    }
}
