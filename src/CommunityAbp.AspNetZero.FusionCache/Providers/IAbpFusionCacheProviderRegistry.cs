namespace CommunityAbp.AspNetZero.FusionCache.Providers;

/// <summary>
/// Defines methods for registering and retrieving distributed and backplane cache providers by name within the
/// FusionCache system.
/// </summary>
/// <remarks>This interface enables dynamic registration and lookup of cache providers, allowing
/// applications to support multiple distributed and backplane caching implementations. Implementations are expected
/// to manage provider lifetimes and ensure that provider names are unique within their respective
/// categories.</remarks>
public interface IAbpFusionCacheProviderRegistry
{
    /// <summary>
    /// Registers a distributed cache provider of the specified type for use with the fusion cache system.
    /// </summary>
    /// <typeparam name="T">The type of the distributed cache provider to register. Must implement the
    /// IAbpFusionCacheDistributedProvider interface.</typeparam>
    void RegisterDistributedProvider<T>() where T : class, IAbpFusionCacheDistributedProvider;

    /// <summary>
    /// Registers a backplane provider of the specified type for use with the FusionCache system.
    /// </summary>
    /// <remarks>Only one backplane provider can be registered at a time. Registering a new provider
    /// will replace any previously registered provider. This method is typically called during application startup
    /// to configure distributed cache synchronization.</remarks>
    /// <typeparam name="T">The type of the backplane provider to register. Must implement the IAbpFusionCacheBackplaneProvider
    /// interface and be a reference type.</typeparam>
    void RegisterBackplaneProvider<T>() where T : class, IAbpFusionCacheBackplaneProvider;

    /// <summary>
    /// Retrieves a distributed cache provider by its name.
    /// </summary>
    /// <param name="providerName">The name of the distributed cache provider to retrieve. Cannot be null or empty.</param>
    /// <returns>An instance of <see cref="IAbpFusionCacheDistributedProvider"/> corresponding to the specified provider
    /// name.</returns>
    IAbpFusionCacheDistributedProvider GetDistributedProvider(string providerName);

    /// <summary>
    /// Retrieves the backplane provider associated with the specified provider name.
    /// </summary>
    /// <param name="providerName">The name of the backplane provider to retrieve. This value is case-sensitive and must correspond to a
    /// registered provider.</param>
    /// <returns>An instance of <see cref="IAbpFusionCacheBackplaneProvider"/> that matches the specified provider name.</returns>
    IAbpFusionCacheBackplaneProvider GetBackplaneProvider(string providerName);

    /// <summary>
    /// Retrieves the names of all distributed providers that are currently available.
    /// </summary>
    /// <returns>An enumerable collection of strings containing the names of available distributed providers. The collection
    /// is empty if no providers are available.</returns>
    IEnumerable<string> GetAvailableDistributedProviders();

    /// <summary>
    /// Retrieves the names of all available backplane providers supported by the system.
    /// </summary>
    /// <returns>An enumerable collection of strings containing the names of available backplane providers. The collection is
    /// empty if no providers are available.</returns>
    IEnumerable<string> GetAvailableBackplaneProviders();
}
