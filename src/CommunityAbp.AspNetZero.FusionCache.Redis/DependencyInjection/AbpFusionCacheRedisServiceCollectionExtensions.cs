using CommunityAbp.AspNetZero.FusionCache.Redis.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace CommunityAbp.AspNetZero.FusionCache.Redis.DependencyInjection
{
    /// <summary>
    /// Provides extension methods for registering FusionCache with Redis integration in an ASP.NET Core application's
    /// dependency injection container.
    /// </summary>
    /// <remarks>This class enables the configuration of FusionCache to use Redis as a distributed cache and,
    /// optionally, as a backplane for cache synchronization across multiple instances. It also configures a JSON
    /// serializer for FusionCache. Use these extensions to simplify the setup of distributed caching and cache
    /// synchronization in applications that require high availability and scalability.</remarks>
    public static class AbpFusionCacheRedisServiceCollectionExtensions
    {
        /// <summary>
        /// Adds FusionCache with Redis integration to the service collection, configuring distributed caching and
        /// optional Redis backplane support.
        /// </summary>
        /// <remarks>This method configures FusionCache to use Redis as the distributed cache provider and
        /// sets up a JSON serializer. If backplane support is enabled in the options, a Redis backplane is also
        /// registered to enable cache synchronization across multiple instances. The Redis connection string is
        /// resolved from the options, the 'Redis' connection string, or the 'Redis:Configuration' configuration key, in
        /// that order.</remarks>
        /// <param name="services">The service collection to which FusionCache and Redis services will be added.</param>
        /// <param name="configuration">The application configuration used to retrieve Redis connection settings.</param>
        /// <param name="optionsAction">An optional action to configure additional FusionCache Redis options before registration.</param>
        /// <returns>The same service collection instance, enabling method chaining.</returns>
        public static IServiceCollection AddAbpFusionCacheRedis(
            this IServiceCollection services,
            IConfiguration configuration,
            Action<AbpFusionCacheRedisOptions>? optionsAction = null)
        {
            var options = new AbpFusionCacheRedisOptions();
            optionsAction?.Invoke(options);

            var connectionString = options.ConnectionString
                ?? configuration.GetConnectionString("Redis")
                ?? configuration["Redis:Configuration"];

            // Add Redis distributed cache
            services.AddStackExchangeRedisCache(redisOptions =>
            {
                redisOptions.Configuration = connectionString;
                redisOptions.InstanceName = options.InstanceName;
            });

            // Add JSON serializer for FusionCache
            services.AddFusionCacheSystemTextJsonSerializer();

            // Add Redis backplane if enabled
            if (options.EnableBackplane)
            {
                services.AddFusionCacheStackExchangeRedisBackplane(backplaneOptions =>
                {
                    backplaneOptions.Configuration = connectionString;

                    // Set channel prefix if configured
                    if (!string.IsNullOrEmpty(options.BackplaneChannelPrefix))
                    {
                        // Ensure ConfigurationOptions exists
                        backplaneOptions.ConfigurationOptions ??= ConfigurationOptions.Parse(connectionString);
                        backplaneOptions.ConfigurationOptions.ChannelPrefix = RedisChannel.Literal(options.BackplaneChannelPrefix);
                    }
                });
            }

            return services;
        }
    }
}
