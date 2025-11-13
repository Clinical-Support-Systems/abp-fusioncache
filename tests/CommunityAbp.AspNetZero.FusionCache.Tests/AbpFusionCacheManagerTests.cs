using Abp.Configuration.Startup;
using Abp.Dependency;
using Abp.Runtime.Caching;
using Abp.Runtime.Caching.Configuration;
using CommunityAbp.AspNetZero.FusionCache.Configuration;
using CommunityAbp.AspNetZero.FusionCache.Runtime.Caching.FusionCache;
using NSubstitute;

namespace CommunityAbp.AspNetZero.FusionCache.Tests;

public class AbpFusionCacheManagerTests : TestBaseWithLocalIocManager
{
    private ICachingConfiguration _configuration = null!;
    private AbpFusionCacheManager _cacheManager = null!;

    [Before(Test)]
    public void Setup()
    {
        // Register a public test implementation of ICachingConfiguration
        if (!LocalIocManager.IsRegistered<ICachingConfiguration>())
        {
            LocalIocManager.Register<ICachingConfiguration, TestCachingConfiguration>(DependencyLifeStyle.Singleton);
        }
        _configuration = LocalIocManager.Resolve<ICachingConfiguration>();
        // Assign a minimal AbpConfiguration
        if (_configuration is TestCachingConfiguration testConfig)
        {
            testConfig.AbpConfiguration = Substitute.For<IAbpStartupConfiguration>();
        }
        _configuration.UseFusionCache();
        // Register a dummy AbpFusionCache for tests
        if (!LocalIocManager.IsRegistered<AbpFusionCache>())
        {
            LocalIocManager.IocContainer.Register(
                Castle.MicroKernel.Registration.Component.For<AbpFusionCache>()
                    .UsingFactoryMethod((kernel, context) =>
                    {
                        var name = context.AdditionalArguments["name"] as string ?? "TestCache";
                        var fusionCache = Substitute.For<ZiggyCreatures.Caching.Fusion.IFusionCache>();
                        var keyNormalizer = Substitute.For<IAbpFusionCacheKeyNormalizer>();
                        var serializer = Substitute.For<IAbpFusionCacheSerializer>();
                        var optionsModifier = Substitute.For<Internal.IAbpMultiTenancyFusionCacheEntryOptionsModifier>();
                        // Ensure any IAbpFusionCacheConfiguration substitute returns a valid AbpFusionCacheOptions
                        var abpFusionCacheConfig = Substitute.For<IAbpFusionCacheConfiguration>();
                        abpFusionCacheConfig.Options.Returns(new Options.AbpFusionCacheOptions());
                        var cache = new TestAbpFusionCache(name, fusionCache, keyNormalizer, serializer, optionsModifier);
                        return cache;
                    })
                    .LifestyleTransient()
            );
        }
        _cacheManager = new AbpFusionCacheManager(LocalIocManager, _configuration);
    }

    [After(Test)]
    public void Cleanup()
    {
        _cacheManager?.Dispose();
    }

    [Test]
    public async Task Constructor_ShouldInitializeWithEmptyCacheCollection()
    {
        var manager = new AbpFusionCacheManager(LocalIocManager, _configuration);
        var caches = manager.GetAllCaches();
        await Assert.That(caches).IsEmpty();
    }

    [Test]
    public async Task GetCache_WithValidName_ShouldReturnCacheInstance()
    {
        var cacheName = "TestCache";
        var cache = _cacheManager.GetCache(cacheName);
        await Assert.That(cache).IsNotNull();
        await Assert.That(cache.Name).IsEqualTo(cacheName);
    }

    [Test]
    public async Task GetCache_WithNullName_ShouldThrowArgumentNullException()
    {
        await Assert.That(() => Task.FromResult(_cacheManager.GetCache(null!))!)
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task GetCache_CalledMultipleTimesWithSameName_ShouldReturnSameInstance()
    {
        var cacheName = "TestCache";
        var cache1 = _cacheManager.GetCache(cacheName);
        var cache2 = _cacheManager.GetCache(cacheName);
        await Assert.That(cache1).IsEqualTo(cache2);
    }

    [Test]
    public async Task GetCache_WithDifferentNames_ShouldReturnDifferentInstances()
    {
        var cacheName1 = "Cache1";
        var cacheName2 = "Cache2";
        var cache1 = _cacheManager.GetCache(cacheName1);
        var cache2 = _cacheManager.GetCache(cacheName2);
        await Assert.That(cache1).IsNotEqualTo(cache2);
    }

    [Test]
    public async Task GetAllCaches_WithNoCaches_ShouldReturnEmptyList()
    {
        var caches = _cacheManager.GetAllCaches();
        await Assert.That(caches).IsEmpty();
        await Assert.That(caches).IsNotNull();
    }

    [Test]
    public async Task GetAllCaches_WithMultipleCaches_ShouldReturnAllCaches()
    {
        var cache1 = _cacheManager.GetCache("Cache1");
        var cache2 = _cacheManager.GetCache("Cache2");
        var cache3 = _cacheManager.GetCache("Cache3");
        var caches = _cacheManager.GetAllCaches();
        await Assert.That(caches.Count).IsEqualTo(3);
        await Assert.That(caches).Contains(cache1);
        await Assert.That(caches).Contains(cache2);
        await Assert.That(caches).Contains(cache3);
    }

    [Test]
    public async Task GetAllCaches_ShouldReturnReadOnlyList()
    {
        _cacheManager.GetCache("TestCache");
        var caches = _cacheManager.GetAllCaches();
        await Assert.That(caches).IsAssignableTo<IReadOnlyList<ICache>>();
    }

    [Test]
    public async Task Dispose_ShouldReleaseAllCaches()
    {
        var cache1 = _cacheManager.GetCache("Cache1");
        var cache2 = _cacheManager.GetCache("Cache2");
        _cacheManager.Dispose();
        await Assert.That(_cacheManager.GetAllCaches()).IsEmpty();
    }

    [Test]
    public async Task Dispose_ShouldClearCacheDictionary()
    {
        _cacheManager.GetCache("TestCache");
        _cacheManager.Dispose();
        var cachesAfterDispose = _cacheManager.GetAllCaches();
        await Assert.That(cachesAfterDispose).IsEmpty();
    }

    [Test]
    public async Task GetCache_IsThreadSafe_WhenCalledConcurrently()
    {
        var cacheName = "ConcurrentCache";
        var tasks = new List<Task<ICache>>();
        for (var i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(() => _cacheManager.GetCache(cacheName)));
        }
        await Task.WhenAll(tasks);
        var results = tasks.Select(t => t.Result).ToList();
        await Assert.That(results.Distinct().Count()).IsEqualTo(1);
    }

    [Test]
    public async Task CreateCacheImplementation_ShouldResolveAbpFusionCacheWithName()
    {
        var cacheName = "TestCache";
        var cache = _cacheManager.GetCache(cacheName);
        await Assert.That(cache).IsNotNull();
        await Assert.That(cache.Name).IsEqualTo(cacheName);
    }

    [Test]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "TUnitAssertions0005:Assert.That(...) should not be used with a constant value", Justification = "<Pending>")]
    public async Task Dispose_CalledMultipleTimes_ShouldNotThrow()
    {
        _cacheManager.GetCache("TestCache");
        _cacheManager.Dispose();
        _cacheManager.Dispose();
        await Assert.That(true).IsTrue();
    }
}
