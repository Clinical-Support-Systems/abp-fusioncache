using CommunityAbp.AspNetZero.FusionCache.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CommunityAbp.AspNetZero.FusionCache.Providers;

/// <summary>
/// Defines a contract for providing and configuring a backplane integration for FusionCache in an ABP application.
/// </summary>
/// <remarks>Implementations of this interface enable distributed cache synchronization by integrating a
/// specific backplane technology (such as Redis or a message bus) with FusionCache. The provider is responsible for
/// registering required services and creating the backplane instance as needed. Typically, only one provider should
/// be active per application instance.</remarks>
public interface IAbpFusionCacheBackplaneProvider
{
    /// <summary>
    /// Gets the name of the data provider associated with the current context.
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
    /// Creates and returns a new backplane instance using the specified service provider.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve dependencies required for the backplane instance. Cannot be null.</param>
    /// <returns>An object representing the created backplane instance, or null if the backplane could not be created.</returns>
    object? CreateBackplane(IServiceProvider serviceProvider);

    /// <summary>
    /// Determines whether the specified configuration is available for use.
    /// </summary>
    /// <param name="configuration">The configuration to check for availability. Cannot be null.</param>
    /// <returns>true if the configuration is available; otherwise, false.</returns>
    bool IsAvailable(IConfiguration configuration);
}
