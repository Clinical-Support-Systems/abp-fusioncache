using CommunityAbp.AspNetZero.FusionCache.Options;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CommunityAbp.AspNetZero.FusionCache.Providers;

/// <summary>
/// Defines an interface for distributed cache providers that can be integrated with FusionCache in an ABP
/// application.
/// </summary>
/// <remarks>Implement this interface to enable support for custom distributed cache backends within the
/// FusionCache infrastructure. Implementations are responsible for registering required services, creating
/// distributed cache instances, and indicating their availability based on configuration.</remarks>
public interface IAbpFusionCacheDistributedProvider
{
    /// <summary>
    /// Gets the name of the underlying data provider.
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Configures application services and registers dependencies using the specified service collection,
    /// configuration, and cache options.
    /// </summary>
    /// <param name="services">The service collection to which application services and dependencies will be added. Must not be null.</param>
    /// <param name="configuration">The application configuration used to retrieve settings required for service registration. Must not be null.</param>
    /// <param name="options">The options used to configure FusionCache integration. Must not be null.</param>
    void ConfigureServices(IServiceCollection services, IConfiguration configuration, AbpFusionCacheOptions options);

    /// <summary>
    /// Creates a new instance of an implementation of the IDistributedCache interface using the specified service
    /// provider.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve dependencies required by the distributed cache implementation. Cannot
    /// be null.</param>
    /// <returns>An instance of IDistributedCache configured with services from the provided service provider.</returns>
    IDistributedCache CreateDistributedCache(IServiceProvider serviceProvider);

    /// <summary>
    /// Determines whether the specified configuration is available for use.
    /// </summary>
    /// <param name="configuration">The configuration to check for availability. Cannot be null.</param>
    /// <returns>true if the configuration is available; otherwise, false.</returns>
    bool IsAvailable(IConfiguration configuration);
}
