namespace CommunityAbp.AspNetZero.FusionCache.Options
{
    /// <summary>
    /// Provides configuration options for integrating FusionCache with an ABP application.
    /// </summary>
    /// <remarks>This class allows customization of FusionCache features, including memory and distributed
    /// caching, fail-safe behavior, cache stampede protection, eager refresh, multi-tenancy support, and backplane
    /// integration. These options control how caching operates within the ABP framework and can be adjusted to fit
    /// specific application requirements.</remarks>
    public class AbpFusionCacheOptions
    {
        /// <summary>
        /// Gets or sets the default duration for which items are cached (L1 memory cache).
        /// </summary>
        /// <remarks>This value determines how long cached items remain valid before expiring, unless an
        /// explicit duration is specified for a particular cache entry.</remarks>
        public TimeSpan DefaultCacheDuration { get; set; } = TimeSpan.FromMinutes(30);

        /// <summary>
        /// Gets or sets a value indicating whether L1 in-memory caching is enabled.
        /// </summary>
        /// <remarks>When enabled, data may be stored in memory to improve performance by reducing
        /// repeated data retrieval operations. Disabling memory caching can be useful in scenarios where data
        /// consistency is critical or memory usage must be minimized.</remarks>
        public bool EnableMemoryCache { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether distributed (L2) caching is enabled.
        /// </summary>
        /// <remarks>When enabled, the application will use a distributed cache provider for storing and
        /// retrieving cached data, allowing cache sharing across multiple instances. This is typically used in
        /// load-balanced or cloud environments to improve scalability and consistency.</remarks>
        public bool EnableDistributedCache { get; set; } = false;

        /// <summary>
        /// Gets or sets the name of the distributed (L2) cache provider to use.
        /// </summary>
        public string DistributedCacheProviderName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether fail-safe mode is enabled.
        /// </summary>
        /// <remarks>When fail-safe mode is enabled, the system will attempt to continue operating in the
        /// event of certain failures, potentially with reduced functionality. Disabling this option may cause the
        /// system to halt on errors instead of attempting recovery.</remarks>
        public bool EnableFailSafe { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether cache stampede protection is enabled.
        /// </summary>
        /// <remarks>When enabled, the cache implements mechanisms to prevent multiple concurrent requests
        /// from triggering redundant data fetches when a cache entry expires. This can help reduce load on the
        /// underlying data source during high-concurrency scenarios.</remarks>
        public bool EnableCacheStampedeProtection { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether eager refresh is enabled for cached credentials.
        /// </summary>
        /// <remarks>When enabled, the system proactively refreshes cached credentials before they expire,
        /// reducing the likelihood of authentication failures due to expired tokens. Disabling this option may result
        /// in credentials being refreshed only when they are about to expire or have already expired.</remarks>
        public bool EnableEagerRefresh { get; set; } = false;

        /// <summary>
        /// Gets or sets the threshold, as a fraction of the token lifetime, at which an access token is proactively
        /// refreshed before expiration.
        /// </summary>
        /// <remarks>A higher value causes tokens to be refreshed closer to their expiration time, while a
        /// lower value triggers earlier refreshes. Typical values are between 0 and 1. Setting this property can help
        /// balance between minimizing unnecessary refreshes and reducing the risk of using expired tokens.</remarks>
        public float EagerRefreshThreshold { get; set; } = 0.9f;

        /// <summary>
        /// Gets or sets a value indicating whether multi-tenancy is enabled for the application.
        /// </summary>
        /// <remarks>When multi-tenancy is enabled, the application can support multiple tenants with
        /// isolated data and configuration. Disabling this option will cause the application to operate in
        /// single-tenant mode.</remarks>
        public bool EnableMultiTenancy { get; set; } = true;

        /// <summary>
        /// Gets or sets the prefix used for cache keys.
        /// </summary>
        public string KeyPrefix { get; set; } = "AbpCache";

        /// <summary>
        /// Gets or sets a value indicating whether the distributed backplane is enabled for cache synchronization.
        /// </summary>
        /// <remarks>When enabled, cache changes are propagated across multiple instances to maintain
        /// consistency in distributed environments. Disabling the backplane may improve performance in single-instance
        /// scenarios but can lead to stale data if multiple instances are used.</remarks>
        public bool EnableBackplane { get; set; } = false;

        /// <summary>
        /// Gets or sets the name of the backplane provider used for distributed cache synchronization.
        /// </summary>
        public string BackplaneProviderName { get; set; }
    }
}
