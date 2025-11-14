using Abp.Dependency;
using CommunityAbp.AspNetZero.FusionCache.Options;
using CommunityAbp.AspNetZero.FusionCache.Providers;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CommunityAbp.AspNetZero.FusionCache.Tests;

public class AbpFusionCacheProviderRegistryTests : TestBaseWithLocalIocManager
{
    private AbpFusionCacheProviderRegistry _registry = null!;

    [Before(Test)]
    public void Setup()
    {
        _registry = new AbpFusionCacheProviderRegistry(LocalIocManager);

        // Register test providers
        LocalIocManager.Register<TestDistributedProvider>(DependencyLifeStyle.Transient);
        LocalIocManager.Register<TestBackplaneProvider>(DependencyLifeStyle.Transient);
        LocalIocManager.Register<AlternativeDistributedProvider>(DependencyLifeStyle.Transient);
    }

    #region RegisterDistributedProvider Tests

    [Test]
    public async Task RegisterDistributedProvider_ShouldAddProviderToRegistry()
    {
        // Act
        _registry.RegisterDistributedProvider<TestDistributedProvider>();

        // Assert
        var providers = _registry.GetAvailableDistributedProviders();
        await Assert.That(providers).Contains("TestDistributed");
    }

    [Test]
    public async Task RegisterDistributedProvider_WithMultipleProviders_ShouldAddAll()
    {
        // Act
        _registry.RegisterDistributedProvider<TestDistributedProvider>();
        _registry.RegisterDistributedProvider<AlternativeDistributedProvider>();

        // Assert
        var providers = _registry.GetAvailableDistributedProviders().ToList();
        await Assert.That(providers.Count).IsEqualTo(2);
        await Assert.That(providers).Contains("TestDistributed");
        await Assert.That(providers).Contains("AlternativeDistributed");
    }

    #endregion

    #region RegisterBackplaneProvider Tests

    [Test]
    public async Task RegisterBackplaneProvider_ShouldAddProviderToRegistry()
    {
        // Act
        _registry.RegisterBackplaneProvider<TestBackplaneProvider>();

        // Assert
        var providers = _registry.GetAvailableBackplaneProviders();
        await Assert.That(providers).Contains("TestBackplane");
    }

    [Test]
    public async Task RegisterBackplaneProvider_CalledTwiceWithSameProvider_ShouldNotDuplicate()
    {
        // Act
        _registry.RegisterBackplaneProvider<TestBackplaneProvider>();
        _registry.RegisterBackplaneProvider<TestBackplaneProvider>();

        // Assert
        var providers = _registry.GetAvailableBackplaneProviders().ToList();
        var count = providers.Count(p => p == "TestBackplane");
        await Assert.That(count).IsEqualTo(1);
    }

    #endregion

    #region GetDistributedProvider Tests

    [Test]
    public async Task GetDistributedProvider_WithRegisteredProvider_ShouldReturnInstance()
    {
        // Arrange
        _registry.RegisterDistributedProvider<TestDistributedProvider>();

        // Act
        var provider = _registry.GetDistributedProvider("TestDistributed");

        // Assert
        await Assert.That(provider).IsNotNull();
        await Assert.That(provider).IsTypeOf(typeof(TestDistributedProvider));
        await Assert.That(provider.ProviderName).IsEqualTo("TestDistributed");
    }

    [Test]
    public async Task GetDistributedProvider_WithUnregisteredProvider_ShouldThrowArgumentException()
    {
        // Act & Assert
        await Assert.That(() => Task.FromResult(_registry.GetDistributedProvider("NonExistent")))
            .Throws<ArgumentException>()
            .With(ex => ex.Message.Contains("Distributed cache provider 'NonExistent' is not registered"));
    }

    [Test]
    public async Task GetDistributedProvider_CalledMultipleTimes_ShouldResolveEachTime()
    {
        // Arrange
        _registry.RegisterDistributedProvider<TestDistributedProvider>();

        // Act
        var provider1 = _registry.GetDistributedProvider("TestDistributed");
        var provider2 = _registry.GetDistributedProvider("TestDistributed");

        // Assert - Both should be valid instances (transient registration)
        await Assert.That(provider1).IsNotNull();
        await Assert.That(provider2).IsNotNull();
        await Assert.That(provider1.ProviderName).IsEqualTo(provider2.ProviderName);
    }

    #endregion

    #region GetBackplaneProvider Tests

    [Test]
    public async Task GetBackplaneProvider_WithRegisteredProvider_ShouldReturnInstance()
    {
        // Arrange
        _registry.RegisterBackplaneProvider<TestBackplaneProvider>();

        // Act
        var provider = _registry.GetBackplaneProvider("TestBackplane");

        // Assert
        await Assert.That(provider).IsNotNull();
        await Assert.That(provider).IsTypeOf(typeof(TestBackplaneProvider));
        await Assert.That(provider.ProviderName).IsEqualTo("TestBackplane");
    }

    [Test]
    public async Task GetBackplaneProvider_WithUnregisteredProvider_ShouldThrowArgumentException()
    {
        // Act & Assert
        await Assert.That(() => Task.FromResult(_registry.GetBackplaneProvider("NonExistent")))
            .Throws<ArgumentException>()
            .With(ex => ex.Message.Contains("Backplane provider 'NonExistent' is not registered"));
    }

    #endregion

    #region GetAvailableDistributedProviders Tests

    [Test]
    public async Task GetAvailableDistributedProviders_WithNoProviders_ShouldReturnEmpty()
    {
        // Act
        var providers = _registry.GetAvailableDistributedProviders();

        // Assert
        await Assert.That(providers).IsEmpty();
    }

    [Test]
    public async Task GetAvailableDistributedProviders_WithProviders_ShouldReturnAllNames()
    {
        // Arrange
        _registry.RegisterDistributedProvider<TestDistributedProvider>();
        _registry.RegisterDistributedProvider<AlternativeDistributedProvider>();

        // Act
        var providers = _registry.GetAvailableDistributedProviders().ToList();

        // Assert
        await Assert.That(providers.Count).IsEqualTo(2);
        await Assert.That(providers).Contains("TestDistributed");
        await Assert.That(providers).Contains("AlternativeDistributed");
    }

    #endregion

    #region GetAvailableBackplaneProviders Tests

    [Test]
    public async Task GetAvailableBackplaneProviders_WithNoProviders_ShouldReturnEmpty()
    {
        // Act
        var providers = _registry.GetAvailableBackplaneProviders();

        // Assert
        await Assert.That(providers).IsEmpty();
    }

    [Test]
    public async Task GetAvailableBackplaneProviders_WithProviders_ShouldReturnAllNames()
    {
        // Arrange
        _registry.RegisterBackplaneProvider<TestBackplaneProvider>();

        // Act
        var providers = _registry.GetAvailableBackplaneProviders().ToList();

        // Assert
        await Assert.That(providers.Count).IsEqualTo(1);
        await Assert.That(providers).Contains("TestBackplane");
    }

    #endregion

    #region Integration Tests

    [Test]
    public async Task Registry_ShouldHandleBothDistributedAndBackplaneProviders()
    {
        // Arrange & Act
        _registry.RegisterDistributedProvider<TestDistributedProvider>();
        _registry.RegisterBackplaneProvider<TestBackplaneProvider>();

        // Assert
        var distributedProviders = _registry.GetAvailableDistributedProviders();
        var backplaneProviders = _registry.GetAvailableBackplaneProviders();

        await Assert.That(distributedProviders).Contains("TestDistributed");
        await Assert.That(backplaneProviders).Contains("TestBackplane");
    }

    [Test]
    public async Task Registry_ShouldKeepDistributedAndBackplaneSeparate()
    {
        // Arrange & Act
        _registry.RegisterDistributedProvider<TestDistributedProvider>();
        _registry.RegisterBackplaneProvider<TestBackplaneProvider>();

        // Assert - Names don't cross-pollinate
        var distributedProviders = _registry.GetAvailableDistributedProviders();
        var backplaneProviders = _registry.GetAvailableBackplaneProviders();

        await Assert.That(distributedProviders).DoesNotContain("TestBackplane");
        await Assert.That(backplaneProviders).DoesNotContain("TestDistributed");
    }

    #endregion

    #region Test Helper Classes

    public class TestDistributedProvider : IAbpFusionCacheDistributedProvider
    {
        public string ProviderName => "TestDistributed";

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration, AbpFusionCacheOptions options)
        {
            // Test implementation
        }

        public IDistributedCache CreateDistributedCache(IServiceProvider serviceProvider)
        {
            return null!;
        }

        public bool IsAvailable(IConfiguration configuration)
        {
            return true;
        }
    }

    public class AlternativeDistributedProvider : IAbpFusionCacheDistributedProvider
    {
        public string ProviderName => "AlternativeDistributed";

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration, AbpFusionCacheOptions options)
        {
            // Test implementation
        }

        public IDistributedCache CreateDistributedCache(IServiceProvider serviceProvider)
        {
            return null!;
        }

        public bool IsAvailable(IConfiguration configuration)
        {
            return true;
        }
    }

    public class TestBackplaneProvider : IAbpFusionCacheBackplaneProvider
    {
        public string ProviderName => "TestBackplane";

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration, AbpFusionCacheOptions options)
        {
            // Test implementation
        }

        public object? CreateBackplane(IServiceProvider serviceProvider)
        {
            return null;
        }

        public bool IsAvailable(IConfiguration configuration)
        {
            return true;
        }
    }

    #endregion
}
