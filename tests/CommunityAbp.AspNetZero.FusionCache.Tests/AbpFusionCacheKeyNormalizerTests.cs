using Abp.Configuration.Startup;
using Abp.Runtime.Session;
using CommunityAbp.AspNetZero.FusionCache.Options;
using CommunityAbp.AspNetZero.FusionCache.Runtime.Caching.FusionCache;
using NSubstitute;

namespace CommunityAbp.AspNetZero.FusionCache.Tests;

public class AbpFusionCacheKeyNormalizerTests
{
    private IMultiTenancyConfig _multiTenancyConfig = null!;
    private IAbpSession _session = null!;
    private AbpFusionCacheOptions _options = null!;
    private AbpFusionCacheKeyNormalizer _normalizer = null!;

    [Before(Test)]
    public void Setup()
    {
        _multiTenancyConfig = Substitute.For<IMultiTenancyConfig>();
        _session = Substitute.For<IAbpSession>();
        _options = new AbpFusionCacheOptions();

        _normalizer = new AbpFusionCacheKeyNormalizer(_multiTenancyConfig, _session, _options);
    }

    #region Basic Normalization Tests

    [Test]
    public async Task NormalizeKey_WithSimpleKey_ShouldReturnKey()
    {
        // Arrange
        var key = "testKey";
        _options.KeyPrefix = string.Empty;
        _options.EnableMultiTenancy = false;

        // Act
        var result = _normalizer.NormalizeKey(key);

        // Assert
        await Assert.That(result).IsEqualTo(key);
    }

    [Test]
    public async Task NormalizeKey_WithNullKey_ShouldThrowArgumentException()
    {
        // Act & Assert
        await Assert.That(() => Task.FromResult(_normalizer.NormalizeKey(null!)))
            .Throws<ArgumentException>()
            .With(ex => ex.Message.Contains("Cache key cannot be null or empty"));
    }

    [Test]
    public async Task NormalizeKey_WithEmptyKey_ShouldThrowArgumentException()
    {
        // Act & Assert
        await Assert.That(() => Task.FromResult(_normalizer.NormalizeKey(string.Empty)))
            .Throws<ArgumentException>()
            .With(ex => ex.Message.Contains("Cache key cannot be null or empty"));
    }

    #endregion

    #region Key Prefix Tests

    [Test]
    public async Task NormalizeKey_WithKeyPrefix_ShouldPrependPrefix()
    {
        // Arrange
        var key = "testKey";
        _options.KeyPrefix = "MyApp";
        _options.EnableMultiTenancy = false;

        // Act
        var result = _normalizer.NormalizeKey(key);

        // Assert
        await Assert.That(result).IsEqualTo("MyApp:testKey");
    }

    [Test]
    public async Task NormalizeKey_WithEmptyKeyPrefix_ShouldNotPrependPrefix()
    {
        // Arrange
        var key = "testKey";
        _options.KeyPrefix = string.Empty;
        _options.EnableMultiTenancy = false;

        // Act
        var result = _normalizer.NormalizeKey(key);

        // Assert
        await Assert.That(result).IsEqualTo("testKey");
    }

    [Test]
    public async Task NormalizeKey_WithNullKeyPrefix_ShouldNotPrependPrefix()
    {
        // Arrange
        var key = "testKey";
        _options.KeyPrefix = null!;
        _options.EnableMultiTenancy = false;

        // Act
        var result = _normalizer.NormalizeKey(key);

        // Assert
        await Assert.That(result).IsEqualTo("testKey");
    }

    [Test]
    public async Task NormalizeKey_WithCustomKeyPrefix_ShouldPrependCorrectly()
    {
        // Arrange
        var key = "userCache";
        _options.KeyPrefix = "Production";
        _options.EnableMultiTenancy = false;

        // Act
        var result = _normalizer.NormalizeKey(key);

        // Assert
        await Assert.That(result).IsEqualTo("Production:userCache");
    }

    #endregion

    #region Multi-Tenancy Tests

    [Test]
    public async Task NormalizeKey_WithMultiTenancyEnabled_AndTenantId_ShouldIncludeTenantId()
    {
        // Arrange
        var key = "testKey";
        var tenantId = 123;
        _options.KeyPrefix = string.Empty;
        _options.EnableMultiTenancy = true;
        _multiTenancyConfig.IsEnabled.Returns(true);
        _session.TenantId.Returns(tenantId);

        // Act
        var result = _normalizer.NormalizeKey(key);

        // Assert
        await Assert.That(result).IsEqualTo("tenant:123:testKey");
    }

    [Test]
    public async Task NormalizeKey_WithMultiTenancyEnabled_AndNoTenantId_ShouldIncludeHost()
    {
        // Arrange
        var key = "testKey";
        _options.KeyPrefix = string.Empty;
        _options.EnableMultiTenancy = true;
        _multiTenancyConfig.IsEnabled.Returns(true);
        _session.TenantId.Returns((int?)null);

        // Act
        var result = _normalizer.NormalizeKey(key);

        // Assert
        await Assert.That(result).IsEqualTo("tenant:host:testKey");
    }

    [Test]
    public async Task NormalizeKey_WithMultiTenancyDisabledInOptions_ShouldNotIncludeTenantId()
    {
        // Arrange
        var key = "testKey";
        var tenantId = 123;
        _options.KeyPrefix = string.Empty;
        _options.EnableMultiTenancy = false;
        _multiTenancyConfig.IsEnabled.Returns(true);
        _session.TenantId.Returns(tenantId);

        // Act
        var result = _normalizer.NormalizeKey(key);

        // Assert
        await Assert.That(result).IsEqualTo("testKey");
    }

    [Test]
    public async Task NormalizeKey_WithMultiTenancyDisabledInConfig_ShouldNotIncludeTenantId()
    {
        // Arrange
        var key = "testKey";
        var tenantId = 123;
        _options.KeyPrefix = string.Empty;
        _options.EnableMultiTenancy = true;
        _multiTenancyConfig.IsEnabled.Returns(false);
        _session.TenantId.Returns(tenantId);

        // Act
        var result = _normalizer.NormalizeKey(key);

        // Assert
        await Assert.That(result).IsEqualTo("testKey");
    }

    [Test]
    public async Task NormalizeKey_WithDifferentTenantIds_ShouldProduceDifferentKeys()
    {
        // Arrange
        var key = "testKey";
        _options.KeyPrefix = string.Empty;
        _options.EnableMultiTenancy = true;
        _multiTenancyConfig.IsEnabled.Returns(true);

        // Tenant 1
        _session.TenantId.Returns(1);
        var result1 = _normalizer.NormalizeKey(key);

        // Tenant 2
        _session.TenantId.Returns(2);
        var result2 = _normalizer.NormalizeKey(key);

        // Assert
        await Assert.That(result1).IsEqualTo("tenant:1:testKey");
        await Assert.That(result2).IsEqualTo("tenant:2:testKey");
        await Assert.That(result1).IsNotEqualTo(result2);
    }

    #endregion

    #region Combined Prefix and Multi-Tenancy Tests

    [Test]
    public async Task NormalizeKey_WithBothPrefixAndTenant_ShouldApplyBothInCorrectOrder()
    {
        // Arrange
        var key = "testKey";
        var tenantId = 456;
        _options.KeyPrefix = "MyApp";
        _options.EnableMultiTenancy = true;
        _multiTenancyConfig.IsEnabled.Returns(true);
        _session.TenantId.Returns(tenantId);

        // Act
        var result = _normalizer.NormalizeKey(key);

        // Assert
        // Expected order: tenant isolation first, then global prefix
        await Assert.That(result).IsEqualTo("tenant:456:MyApp:testKey");
    }

    [Test]
    public async Task NormalizeKey_WithBothPrefixAndHost_ShouldApplyBothInCorrectOrder()
    {
        // Arrange
        var key = "testKey";
        _options.KeyPrefix = "MyApp";
        _options.EnableMultiTenancy = true;
        _multiTenancyConfig.IsEnabled.Returns(true);
        _session.TenantId.Returns((int?)null);

        // Act
        var result = _normalizer.NormalizeKey(key);

        // Assert
        await Assert.That(result).IsEqualTo("tenant:host:MyApp:testKey");
    }

    #endregion

    #region Isolation Tests

    [Test]
    public async Task NormalizeKey_ShouldIsolateTenantCaches()
    {
        // Arrange
        var key = "userSettings";
        _options.KeyPrefix = "App";
        _options.EnableMultiTenancy = true;
        _multiTenancyConfig.IsEnabled.Returns(true);

        // Tenant 1
        _session.TenantId.Returns(1);
        var tenant1Key = _normalizer.NormalizeKey(key);

        // Tenant 2
        _session.TenantId.Returns(2);
        var tenant2Key = _normalizer.NormalizeKey(key);

        // Host
        _session.TenantId.Returns((int?)null);
        var hostKey = _normalizer.NormalizeKey(key);

        // Assert - All keys should be different
        await Assert.That(tenant1Key).IsNotEqualTo(tenant2Key);
        await Assert.That(tenant1Key).IsNotEqualTo(hostKey);
        await Assert.That(tenant2Key).IsNotEqualTo(hostKey);

        await Assert.That(tenant1Key).IsEqualTo("tenant:1:App:userSettings");
        await Assert.That(tenant2Key).IsEqualTo("tenant:2:App:userSettings");
        await Assert.That(hostKey).IsEqualTo("tenant:host:App:userSettings");
    }

    #endregion

    #region Special Characters Tests

    [Test]
    public async Task NormalizeKey_WithSpecialCharactersInKey_ShouldPreserveCharacters()
    {
        // Arrange
        var key = "user:123:settings";
        _options.KeyPrefix = string.Empty;
        _options.EnableMultiTenancy = false;

        // Act
        var result = _normalizer.NormalizeKey(key);

        // Assert
        await Assert.That(result).IsEqualTo(key);
    }

    [Test]
    public async Task NormalizeKey_WithWhitespaceInKey_ShouldPreserveWhitespace()
    {
        // Arrange
        var key = "user settings";
        _options.KeyPrefix = string.Empty;
        _options.EnableMultiTenancy = false;

        // Act
        var result = _normalizer.NormalizeKey(key);

        // Assert
        await Assert.That(result).IsEqualTo(key);
    }

    [Test]
    public async Task NormalizeKey_WithComplexKey_ShouldHandleCorrectly()
    {
        // Arrange
        var key = "cache:user:123:profile:settings";
        _options.KeyPrefix = "Production";
        _options.EnableMultiTenancy = true;
        _multiTenancyConfig.IsEnabled.Returns(true);
        _session.TenantId.Returns(999);

        // Act
        var result = _normalizer.NormalizeKey(key);

        // Assert
        await Assert.That(result).IsEqualTo("tenant:999:Production:cache:user:123:profile:settings");
    }

    #endregion

    #region Consistency Tests

    [Test]
    public async Task NormalizeKey_CalledMultipleTimesWithSameParams_ShouldReturnSameResult()
    {
        // Arrange
        var key = "testKey";
        _options.KeyPrefix = "MyApp";
        _options.EnableMultiTenancy = true;
        _multiTenancyConfig.IsEnabled.Returns(true);
        _session.TenantId.Returns(42);

        // Act
        var result1 = _normalizer.NormalizeKey(key);
        var result2 = _normalizer.NormalizeKey(key);
        var result3 = _normalizer.NormalizeKey(key);

        // Assert
        await Assert.That(result1).IsEqualTo(result2);
        await Assert.That(result2).IsEqualTo(result3);
    }

    [Test]
    public async Task NormalizeKey_WithLargeDefaultTenantId_ShouldHandleCorrectly()
    {
        // Arrange
        var key = "testKey";
        _options.KeyPrefix = string.Empty;
        _options.EnableMultiTenancy = true;
        _multiTenancyConfig.IsEnabled.Returns(true);
        _session.TenantId.Returns(int.MaxValue);

        // Act
        var result = _normalizer.NormalizeKey(key);

        // Assert
        await Assert.That(result).IsEqualTo($"tenant:{int.MaxValue}:testKey");
    }

    #endregion
}
