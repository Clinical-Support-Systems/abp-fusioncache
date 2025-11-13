using CommunityAbp.AspNetZero.FusionCache.Options;
using CommunityAbp.AspNetZero.FusionCache.Providers;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CommunityAbp.AspNetZero.FusionCache.Redis.Providers;

/// <summary>
/// Provides a FusionCache distributed cache provider implementation that uses Redis as the underlying storage.
/// </summary>
/// <remarks>This provider integrates with the StackExchange.Redis library to enable distributed caching
/// with Redis in FusionCache-based applications. It retrieves the Redis connection string from the application's
/// configuration and registers the necessary services for Redis-backed caching. Use this provider when you want to
/// leverage Redis for distributed cache scenarios in your application.</remarks>
public class AbpFusionCacheRedisProvider : IAbpFusionCacheDistributedProvider
{
    /// <summary>
    /// Gets the name of the data provider used by this instance.
    /// </summary>
    public string ProviderName => "Redis";

    /// <summary>
    /// Configures Redis distributed caching services using the specified configuration and options.
    /// </summary>
    /// <remarks>This method registers StackExchange.Redis as the distributed cache provider. The
    /// Redis connection string is obtained from the provided configuration, and the cache instance name is set
    /// using the specified options.</remarks>
    /// <param name="services">The service collection to which the Redis cache services will be added.</param>
    /// <param name="configuration">The application configuration used to retrieve the Redis connection string.</param>
    /// <param name="options">The options used to configure the Redis cache instance, including the key prefix.</param>
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration, AbpFusionCacheOptions options)
    {
        var connectionString = GetConnectionString(configuration);

        services.AddStackExchangeRedisCache(redisOptions =>
        {
            redisOptions.Configuration = connectionString;
            redisOptions.InstanceName = options.KeyPrefix;
        });
    }

    /// <summary>
    /// Retrieves an instance of the distributed cache service from the specified service provider.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve the <see cref="IDistributedCache"/> implementation. Must not be null.</param>
    /// <returns>An instance of <see cref="IDistributedCache"/> obtained from the service provider.</returns>
    public IDistributedCache CreateDistributedCache(IServiceProvider serviceProvider)
    {
        return serviceProvider.GetRequiredService<IDistributedCache>();
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
