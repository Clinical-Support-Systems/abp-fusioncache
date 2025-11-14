using System.Collections.Concurrent;
using Abp.Dependency;

namespace CommunityAbp.AspNetZero.FusionCache.Providers;

/// <summary>
/// Provides a registry for managing and resolving distributed cache and backplane providers used by FusionCache in
/// an ABP-based application.
/// </summary>
/// <remarks>This registry allows for the registration and retrieval of distributed cache and backplane
/// providers by name. It is typically used to support extensibility and dynamic selection of caching providers at
/// runtime. The registry is intended to be used as a singleton and is integrated with the ABP dependency injection
/// system.</remarks>
public class AbpFusionCacheProviderRegistry : IAbpFusionCacheProviderRegistry, ISingletonDependency
{
    private readonly ConcurrentDictionary<string, Type> _distributedProviders;
    private readonly ConcurrentDictionary<string, Type> _backplaneProviders;
    private readonly IIocManager _iocManager;

    /// <summary>
    /// Initializes a new instance of the AbpFusionCacheProviderRegistry class using the specified IoC manager.
    /// </summary>
    /// <param name="iocManager">The IoC manager used to resolve and manage cache provider dependencies. Cannot be null.</param>
    public AbpFusionCacheProviderRegistry(IIocManager iocManager)
    {
        _iocManager = iocManager;
        _distributedProviders = new ConcurrentDictionary<string, Type>();
        _backplaneProviders = new ConcurrentDictionary<string, Type>();
    }

    /// <summary>
    /// Registers a distributed cache provider of the specified type for use with the FusionCache system.
    /// </summary>
    /// <remarks>This method associates the provider's name with its type and registers it with
    /// transient lifetime in the dependency injection container. Use this method to make a custom distributed
    /// provider available for distributed caching operations.</remarks>
    /// <typeparam name="T">The type of the distributed cache provider to register. Must implement the
    /// IAbpFusionCacheDistributedProvider interface and be a reference type.</typeparam>
    public void RegisterDistributedProvider<T>() where T : class, IAbpFusionCacheDistributedProvider
    {
        var instance = _iocManager.Resolve<T>();
        _distributedProviders.TryAdd(instance.ProviderName, typeof(T));
        if (!_iocManager.IsRegistered<T>())
        {
            _iocManager.Register<T>(DependencyLifeStyle.Transient);
        }
    }

    /// <summary>
    /// Registers a backplane provider of the specified type for use with the FusionCache system.
    /// </summary>
    /// <remarks>This method associates the provider's name with its type and registers it with the
    /// dependency injection container using a transient lifetime. Subsequent calls with the same provider name will
    /// not overwrite existing registrations.</remarks>
    /// <typeparam name="T">The type of the backplane provider to register. Must implement the IAbpFusionCacheBackplaneProvider
    /// interface and be a reference type.</typeparam>
    public void RegisterBackplaneProvider<T>() where T : class, IAbpFusionCacheBackplaneProvider
    {
        // Use Activator to get the provider name without requiring IoC registration
        var instance = Activator.CreateInstance<T>();
        _backplaneProviders.TryAdd(instance.ProviderName, typeof(T));
        if (!_iocManager.IsRegistered<T>())
        {
            _iocManager.Register<T>(DependencyLifeStyle.Transient);
        }
    }

    /// <summary>
    /// Retrieves a distributed cache provider instance by its registered name.
    /// </summary>
    /// <remarks>The returned provider is resolved from the dependency injection container. Each call
    /// returns a new or existing instance according to the provider's registration.</remarks>
    /// <param name="providerName">The name of the distributed cache provider to retrieve. This value must correspond to a provider that has
    /// been registered.</param>
    /// <returns>An instance of the distributed cache provider associated with the specified name.</returns>
    /// <exception cref="ArgumentException">Thrown if a distributed cache provider with the specified name is not registered.</exception>
    public IAbpFusionCacheDistributedProvider GetDistributedProvider(string providerName)
    {
        if (_distributedProviders.TryGetValue(providerName, out var providerType))
        {
            return (IAbpFusionCacheDistributedProvider)_iocManager.Resolve(providerType);
        }

        throw new ArgumentException($"Distributed cache provider '{providerName}' is not registered.");
    }

    /// <summary>
    /// Retrieves an instance of the backplane provider associated with the specified provider name.
    /// </summary>
    /// <param name="providerName">The name of the backplane provider to retrieve. This value must correspond to a registered provider.</param>
    /// <returns>An instance of the backplane provider that matches the specified provider name.</returns>
    /// <exception cref="ArgumentException">Thrown if a backplane provider with the specified name is not registered.</exception>
    public IAbpFusionCacheBackplaneProvider GetBackplaneProvider(string providerName)
    {
        if (_backplaneProviders.TryGetValue(providerName, out var providerType))
        {
            return (IAbpFusionCacheBackplaneProvider)_iocManager.Resolve(providerType);
        }

        throw new ArgumentException($"Backplane provider '{providerName}' is not registered.");
    }

    /// <summary>
    /// Retrieves the names of all available distributed providers.
    /// </summary>
    /// <returns>An enumerable collection of strings containing the names of the available distributed providers. The
    /// collection is empty if no providers are available.</returns>
    public IEnumerable<string> GetAvailableDistributedProviders()
    {
        return _distributedProviders.Keys.ToList();
    }

    /// <summary>
    /// Gets the names of all available backplane providers.
    /// </summary>
    /// <returns>An enumerable collection of strings containing the names of the available backplane providers. The
    /// collection is empty if no providers are available.</returns>
    public IEnumerable<string> GetAvailableBackplaneProviders()
    {
        return _backplaneProviders.Keys.ToList();
    }
}
