namespace CommunityAbp.AspNetZero.FusionCache.Redis.Configuration
{
    /// <summary>
    /// Provides configuration options for integrating Redis with FusionCache in an ABP application.
    /// </summary>
    /// <remarks>These options allow customization of the Redis connection, database selection, backplane
    /// usage, and related timeouts for distributed caching scenarios. Adjust these settings to match your application's
    /// Redis deployment and performance requirements.</remarks>
    public class AbpFusionCacheRedisOptions
    {
        /// <summary>
        /// Gets or sets the connection string used to establish a connection to the data source.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the index of the logical database to be used for operations.
        /// </summary>
        /// <remarks>The database index determines which logical database is selected when connecting to
        /// the data store. The default value is 0. Changing this property affects which database subsequent operations
        /// will target.</remarks>
        public int Database { get; set; } = 0;

        /// <summary>
        /// Gets or sets a value indicating whether the distributed backplane is enabled for cache synchronization.
        /// </summary>
        /// <remarks>When enabled, cache changes are propagated across multiple instances to ensure
        /// consistency in distributed environments. Use this option when running the application in a multi-server or
        /// clustered setup.</remarks>
        public bool EnableBackplane { get; set; } = false;

        /// <summary>
        /// Gets or sets the prefix used for all backplane channel names.
        /// </summary>
        /// <remarks>Changing this value allows multiple applications to share the same backplane
        /// infrastructure without interfering with each other's channels. Ensure that the prefix is unique within the
        /// shared environment to avoid cross-application message delivery.</remarks>
        public string BackplaneChannelPrefix { get; set; } = "AbpFusionCache";

        /// <summary>
        /// Gets or sets the maximum amount of time to wait when establishing a connection before timing out.
        /// </summary>
        public TimeSpan ConnectTimeout { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Gets or sets the maximum duration to wait for a synchronous operation to complete before timing out.
        /// </summary>
        public TimeSpan SyncTimeout { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Gets or sets the name of the cache instance.
        /// </summary>
        public string InstanceName { get; set; } = "AbpFusionCache";
    }
}
