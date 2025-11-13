using System.Collections.Concurrent;
using Abp;
using Abp.Dependency;
using Abp.Runtime.Caching;
using Abp.Runtime.Caching.Configuration;

namespace CommunityAbp.AspNetZero.FusionCache.Runtime.Caching.FusionCache;

/// <summary>
/// Provides a cache manager that creates and manages named cache instances using FusionCache within the ABP
/// framework.
/// </summary>
/// <remarks>AbpFusionCacheManager implements the ICacheManager interface and is registered as a singleton
/// dependency. It creates cache instances on demand and applies any configured cache initializers. All created
/// caches are tracked and can be disposed of when the manager is disposed. This class is thread-safe and intended
/// for use in applications that require centralized cache management.</remarks>
public class AbpFusionCacheManager : ICacheManager, ISingletonDependency
{
    private readonly ConcurrentDictionary<string, ICache> _caches;
    private readonly IIocManager _iocManager;
    private readonly ICachingConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the AbpFusionCacheManager class with the specified dependency injection
    /// manager and caching configuration.
    /// </summary>
    /// <param name="iocManager">The dependency injection manager used to resolve cache dependencies.</param>
    /// <param name="configuration">The configuration settings that define cache behavior and options.</param>
    public AbpFusionCacheManager(IIocManager iocManager, ICachingConfiguration configuration)
    {
        _iocManager = iocManager;
        _configuration = configuration;
        _caches = new ConcurrentDictionary<string, ICache>();
    }

    /// <summary>
    /// Retrieves a read-only list containing all registered cache instances.
    /// </summary>
    /// <returns>A read-only list of <see cref="ICache"/> objects representing all caches currently managed by this instance.
    /// The list will be empty if no caches are registered.</returns>
    public IReadOnlyList<ICache> GetAllCaches()
    {
        return _caches.Values.ToList().AsReadOnly();
    }

    /// <summary>
    /// Retrieves a cache instance with the specified name, creating and configuring it if it does not already
    /// exist.
    /// </summary>
    /// <remarks>If a cache with the specified name does not exist, a new cache is created and
    /// configured using any applicable configurators. This method is thread-safe and ensures that only one cache
    /// instance is created per unique name.</remarks>
    /// <param name="name">The unique name of the cache to retrieve. Cannot be null.</param>
    /// <returns>An <see cref="ICache"/> instance associated with the specified name. The same instance is returned for
    /// subsequent calls with the same name.</returns>
    public ICache GetCache(string name)
    {
        Check.NotNull(name, nameof(name));

        return _caches.GetOrAdd(name, (cacheName) =>
        {
            var cache = CreateCacheImplementation(cacheName);

            var configurators = _configuration.Configurators.Where(c => c.CacheName == null || c.CacheName == cacheName);
            foreach (var configurator in configurators)
            {
                configurator.InitAction?.Invoke(cache);
            }

            return cache;
        });
    }

    protected virtual ICache CreateCacheImplementation(string name)
    {
        return _iocManager.Resolve<AbpFusionCache>(new { name });
    }

    public void Dispose()
    {
        DisposeCaches();
    }

    protected virtual void DisposeCaches()
    {
        foreach (var cache in _caches.Values)
        {
            _iocManager.Release(cache);
        }

        _caches.Clear();
    }
}
