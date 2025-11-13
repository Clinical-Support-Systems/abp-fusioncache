using CommunityAbp.AspNetZero.FusionCache.Options;
using CommunityAbp.AspNetZero.FusionCache.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using ZiggyCreatures.Caching.Fusion.Backplane;

namespace CommunityAbp.AspNetZero.FusionCache.Redis.Providers;

/// <summary>
/// Provides a FusionCache backplane provider that integrates with Redis for distributed cache synchronization.
/// </summary>
/// <remarks>This provider enables FusionCache to use Redis as a backplane, allowing cache events to be
/// propagated across multiple application instances. It supports multi-tenancy scenarios by applying a channel
/// prefix when enabled in the options. The provider requires a valid Redis connection string to be configured in
/// the application's configuration sources.</remarks>
public class AbpFusionCacheRedisBackplaneProvider : IAbpFusionCacheBackplaneProvider
{
    /// <summary>
    /// Gets the name of the data provider used by this instance.
    /// </summary>
    public string ProviderName => "Redis";

    /// <summary>
    /// Configures the FusionCache Redis backplane services for the application, enabling distributed cache
    /// synchronization and optional multi-tenancy support.
    /// </summary>
    /// <remarks>This method sets up the FusionCache backplane using StackExchange.Redis, allowing
    /// cache events to be propagated across multiple application instances. If multi-tenancy is enabled in the
    /// options, a channel prefix is applied to isolate cache events per tenant. Ensure that the configuration
    /// contains a valid Redis connection string before calling this method.</remarks>
    /// <param name="services">The service collection to which the FusionCache Redis backplane services will be added.</param>
    /// <param name="configuration">The application configuration used to retrieve the Redis connection string and related settings.</param>
    /// <param name="options">The options that control FusionCache backplane behavior, including multi-tenancy and key prefix settings.</param>
    /// <exception cref="InvalidOperationException">Thrown if the Redis connection string is not configured in the application settings.</exception>
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration, AbpFusionCacheOptions options)
    {
        var connectionString = GetConnectionString(configuration);

        if (!IsAvailable(configuration))
        {
            throw new InvalidOperationException("Redis connection string is not configured for backplane.");
        }

        // Configure Redis backplane for FusionCache
        services.AddFusionCacheStackExchangeRedisBackplane(backplaneOptions =>
        {
            backplaneOptions.Configuration = connectionString;

            // Handle multi-tenancy channel prefix
            if (options.EnableMultiTenancy)
            {
                var channelPrefix = $"{options.KeyPrefix}:backplane";

                // Create ConfigurationOptions if it doesn't exist
                if (backplaneOptions.ConfigurationOptions == null)
                {
                    backplaneOptions.ConfigurationOptions = new ConfigurationOptions();
                    // Parse the connection string into the ConfigurationOptions
                    backplaneOptions.ConfigurationOptions = ConfigurationOptions.Parse(connectionString);
                }

                // Set the channel prefix
                backplaneOptions.ConfigurationOptions.ChannelPrefix = RedisChannel.Literal(channelPrefix);
            }
        });
    }

    /// <summary>
    /// Creates and returns an instance of the FusionCache backplane from the specified service provider.
    /// </summary>
    /// <remarks>This method relies on the backplane being registered in the service provider,
    /// typically via AddFusionCacheStackExchangeRedisBackplane. If the backplane is not registered, the method
    /// returns null.</remarks>
    /// <param name="serviceProvider">The service provider used to resolve the FusionCache backplane instance. Cannot be null.</param>
    /// <returns>An instance of the FusionCache backplane if registered; otherwise, null.</returns>
    public object? CreateBackplane(IServiceProvider serviceProvider)
    {
        // The backplane is automatically registered by AddFusionCacheStackExchangeRedisBackplane
        // FusionCache will resolve it automatically when needed
        return serviceProvider.GetService<IFusionCacheBackplane>();
    }

    /// <summary>
    /// Determines whether a valid connection string is available in the specified configuration.
    /// </summary>
    /// <param name="configuration">The configuration source to search for a connection string. Cannot be null.</param>
    /// <returns><see langword="true"/> if a non-empty connection string is found in the configuration; otherwise, <see
    /// langword="false"/>.</returns>
    public bool IsAvailable(IConfiguration configuration)
    {
        var connectionString = GetConnectionString(configuration);
        return !string.IsNullOrEmpty(connectionString);
    }

    private static string? GetConnectionString(IConfiguration configuration)
    {
        return configuration.GetConnectionString("Redis")
            ?? configuration["Redis:Configuration"]
            ?? configuration["Redis:ConnectionString"];
    }
}
