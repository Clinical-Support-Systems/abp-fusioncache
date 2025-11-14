using Abp.Configuration.Startup;
using Abp.Runtime.Session;
using CommunityAbp.AspNetZero.FusionCache.Internal;
using CommunityAbp.AspNetZero.FusionCache.Options;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using ZiggyCreatures.Caching.Fusion;

namespace CommunityAbp.AspNetZero.FusionCache.Tests;

public class AbpMultiTenancyFusionCacheEntryOptionsModifierTests
{
    private IMultiTenancyConfig _multiTenancyConfig = null!;
    private IAbpSession _session = null!;
    private AbpFusionCacheOptions _cacheOptions = null!;
    private AbpMultiTenancyFusionCacheEntryOptionsModifier _modifier = null!;

    [Before(Test)]
    public void Setup()
    {
        _multiTenancyConfig = Substitute.For<IMultiTenancyConfig>();
        _session = Substitute.For<IAbpSession>();
        _cacheOptions = new AbpFusionCacheOptions();

        _modifier = new AbpMultiTenancyFusionCacheEntryOptionsModifier(
            _multiTenancyConfig,
            _session,
            _cacheOptions);
    }

    #region Multi-Tenancy Disabled Tests

    [Test]
    public async Task ModifyOptions_WhenMultiTenancyDisabled_ShouldReturnOriginalOptions()
    {
        // Arrange
        var originalOptions = new FusionCacheEntryOptions(TimeSpan.FromMinutes(30));
        var cacheKey = "testKey";
        _cacheOptions.EnableMultiTenancy = false;
        _multiTenancyConfig.IsEnabled.Returns(true);

        // Act
        var result = _modifier.ModifyOptions(originalOptions, cacheKey);

        // Assert
        await Assert.That(result.Duration).IsEqualTo(originalOptions.Duration);
    }

    [Test]
    public async Task ModifyOptions_WhenMultiTenancyNotEnabledInConfig_ShouldReturnOriginalOptions()
    {
        // Arrange
        var originalOptions = new FusionCacheEntryOptions(TimeSpan.FromMinutes(30));
        var cacheKey = "testKey";
        _cacheOptions.EnableMultiTenancy = true;
        _multiTenancyConfig.IsEnabled.Returns(false);

        // Act
        var result = _modifier.ModifyOptions(originalOptions, cacheKey);

        // Assert
        await Assert.That(result.Duration).IsEqualTo(originalOptions.Duration);
    }

    #endregion

    #region Tenant-Specific Options Tests

    [Test]
    public async Task ModifyOptions_ForTenant_ShouldReduceCacheDurationBy25Percent()
    {
        // Arrange
        var originalDuration = TimeSpan.FromMinutes(100);
        var originalOptions = new FusionCacheEntryOptions(originalDuration);
        var cacheKey = "testKey";
        var tenantId = 123;

        _cacheOptions.EnableMultiTenancy = true;
        _multiTenancyConfig.IsEnabled.Returns(true);
        _session.TenantId.Returns(tenantId);

        // Act
        var result = _modifier.ModifyOptions(originalOptions, cacheKey);

        // Assert
        var expectedDuration = TimeSpan.FromMinutes(75); // 75% of original
        await Assert.That(result.Duration).IsEqualTo(expectedDuration);
    }

    [Test]
    public async Task ModifyOptions_ForTenant_ShouldSetHighPriority()
    {
        // Arrange
        var originalOptions = new FusionCacheEntryOptions(TimeSpan.FromMinutes(30));
        var cacheKey = "testKey";
        var tenantId = 123;

        _cacheOptions.EnableMultiTenancy = true;
        _multiTenancyConfig.IsEnabled.Returns(true);
        _session.TenantId.Returns(tenantId);

        // Act
        var result = _modifier.ModifyOptions(originalOptions, cacheKey);

        // Assert
        await Assert.That(result.Priority).IsEqualTo(CacheItemPriority.High);
    }

    [Test]
    public async Task ModifyOptions_ForTenant_WhenFailSafeNotEnabled_ShouldEnableFailSafe()
    {
        // Arrange
        var originalOptions = new FusionCacheEntryOptions(TimeSpan.FromMinutes(30));
        originalOptions.SetFailSafe(false);
        var cacheKey = "testKey";
        var tenantId = 123;

        _cacheOptions.EnableMultiTenancy = true;
        _multiTenancyConfig.IsEnabled.Returns(true);
        _session.TenantId.Returns(tenantId);

        // Act
        var result = _modifier.ModifyOptions(originalOptions, cacheKey);

        // Assert
        await Assert.That(result.IsFailSafeEnabled).IsTrue();
        await Assert.That(result.FailSafeMaxDuration).IsEqualTo(TimeSpan.FromHours(1));
    }

    [Test]
    public async Task ModifyOptions_ForTenant_WhenFailSafeAlreadyEnabled_ShouldNotChangeFailSafe()
    {
        // Arrange
        var originalOptions = new FusionCacheEntryOptions(TimeSpan.FromMinutes(30));
        var customFailSafeDuration = TimeSpan.FromHours(2);
        originalOptions.SetFailSafe(true, customFailSafeDuration);
        var cacheKey = "testKey";
        var tenantId = 123;

        _cacheOptions.EnableMultiTenancy = true;
        _multiTenancyConfig.IsEnabled.Returns(true);
        _session.TenantId.Returns(tenantId);

        // Act
        var result = _modifier.ModifyOptions(originalOptions, cacheKey);

        // Assert
        await Assert.That(result.IsFailSafeEnabled).IsTrue();
        // Should preserve the original fail-safe duration
        await Assert.That(result.FailSafeMaxDuration).IsEqualTo(customFailSafeDuration);
    }

    [Test]
    public async Task ModifyOptions_ForTenant_WhenEagerRefreshEnabled_ShouldSetEagerRefresh()
    {
        // Arrange
        var originalOptions = new FusionCacheEntryOptions(TimeSpan.FromMinutes(30));
        var cacheKey = "testKey";
        var tenantId = 123;

        _cacheOptions.EnableMultiTenancy = true;
        _cacheOptions.EnableEagerRefresh = true;
        _cacheOptions.EagerRefreshThreshold = 0.8f;
        _multiTenancyConfig.IsEnabled.Returns(true);
        _session.TenantId.Returns(tenantId);

        // Act
        var result = _modifier.ModifyOptions(originalOptions, cacheKey);

        // Assert
        await Assert.That(result.EagerRefreshThreshold).IsEqualTo(0.8f);
    }

    [Test]
    public async Task ModifyOptions_ForTenant_WhenCacheStampedeProtectionEnabled_ShouldSetJitter()
    {
        // Arrange
        var originalOptions = new FusionCacheEntryOptions(TimeSpan.FromMinutes(30));
        var cacheKey = "testKey";
        var tenantId = 50; // Will produce jitter of (50 % 100) + 50 = 100ms

        _cacheOptions.EnableMultiTenancy = true;
        _cacheOptions.EnableCacheStampedeProtection = true;
        _multiTenancyConfig.IsEnabled.Returns(true);
        _session.TenantId.Returns(tenantId);

        // Act
        var result = _modifier.ModifyOptions(originalOptions, cacheKey);

        // Assert
        await Assert.That(result.JitterMaxDuration).IsEqualTo(TimeSpan.FromMilliseconds(100));
    }

    [Test]
    public async Task ModifyOptions_ForTenant_WithDifferentTenantIds_ShouldProduceDifferentJitter()
    {
        // Arrange
        var originalOptions = new FusionCacheEntryOptions(TimeSpan.FromMinutes(30));
        var cacheKey = "testKey";

        _cacheOptions.EnableMultiTenancy = true;
        _cacheOptions.EnableCacheStampedeProtection = true;
        _multiTenancyConfig.IsEnabled.Returns(true);

        // Tenant 1 (ID: 10 -> jitter: 60ms)
        _session.TenantId.Returns(10);
        var result1 = _modifier.ModifyOptions(originalOptions, cacheKey);

        // Tenant 2 (ID: 90 -> jitter: 140ms)
        _session.TenantId.Returns(90);
        var result2 = _modifier.ModifyOptions(originalOptions, cacheKey);

        // Assert
        await Assert.That(result1.JitterMaxDuration).IsEqualTo(TimeSpan.FromMilliseconds(60));
        await Assert.That(result2.JitterMaxDuration).IsEqualTo(TimeSpan.FromMilliseconds(140));
        await Assert.That(result1.JitterMaxDuration).IsNotEqualTo(result2.JitterMaxDuration);
    }

    #endregion

    #region Host-Specific Options Tests

    [Test]
    public async Task ModifyOptions_ForHost_ShouldIncreaseCacheDurationBy50Percent()
    {
        // Arrange
        var originalDuration = TimeSpan.FromMinutes(100);
        var originalOptions = new FusionCacheEntryOptions(originalDuration);
        var cacheKey = "testKey";

        _cacheOptions.EnableMultiTenancy = true;
        _multiTenancyConfig.IsEnabled.Returns(true);
        _session.TenantId.Returns((int?)null); // Host context

        // Act
        var result = _modifier.ModifyOptions(originalOptions, cacheKey);

        // Assert
        var expectedDuration = TimeSpan.FromMinutes(150); // 150% of original
        await Assert.That(result.Duration).IsEqualTo(expectedDuration);
    }

    [Test]
    public async Task ModifyOptions_ForHost_ShouldSetNormalPriority()
    {
        // Arrange
        var originalOptions = new FusionCacheEntryOptions(TimeSpan.FromMinutes(30));
        var cacheKey = "testKey";

        _cacheOptions.EnableMultiTenancy = true;
        _multiTenancyConfig.IsEnabled.Returns(true);
        _session.TenantId.Returns((int?)null);

        // Act
        var result = _modifier.ModifyOptions(originalOptions, cacheKey);

        // Assert
        await Assert.That(result.Priority).IsEqualTo(CacheItemPriority.Normal);
    }

    [Test]
    public async Task ModifyOptions_ForHost_WhenFailSafeNotEnabled_ShouldEnableWithLongerDuration()
    {
        // Arrange
        var originalOptions = new FusionCacheEntryOptions(TimeSpan.FromMinutes(30));
        originalOptions.SetFailSafe(false);
        var cacheKey = "testKey";

        _cacheOptions.EnableMultiTenancy = true;
        _multiTenancyConfig.IsEnabled.Returns(true);
        _session.TenantId.Returns((int?)null);

        // Act
        var result = _modifier.ModifyOptions(originalOptions, cacheKey);

        // Assert
        await Assert.That(result.IsFailSafeEnabled).IsTrue();
        await Assert.That(result.FailSafeMaxDuration).IsEqualTo(TimeSpan.FromHours(4));
    }

    [Test]
    public async Task ModifyOptions_ForHost_WhenEagerRefreshEnabled_ShouldAdjustThreshold()
    {
        // Arrange
        var originalOptions = new FusionCacheEntryOptions(TimeSpan.FromMinutes(30));
        var cacheKey = "testKey";

        _cacheOptions.EnableMultiTenancy = true;
        _cacheOptions.EnableEagerRefresh = true;
        _cacheOptions.EagerRefreshThreshold = 0.8f;
        _multiTenancyConfig.IsEnabled.Returns(true);
        _session.TenantId.Returns((int?)null);

        // Act
        var result = _modifier.ModifyOptions(originalOptions, cacheKey);

        // Assert
        // Threshold should be increased by 0.1, but capped at 0.95
        await Assert.That(result.EagerRefreshThreshold).IsEqualTo(0.9f);
    }

    [Test]
    public async Task ModifyOptions_ForHost_WhenEagerRefreshThresholdHigh_ShouldCapAt095()
    {
        // Arrange
        var originalOptions = new FusionCacheEntryOptions(TimeSpan.FromMinutes(30));
        var cacheKey = "testKey";

        _cacheOptions.EnableMultiTenancy = true;
        _cacheOptions.EnableEagerRefresh = true;
        _cacheOptions.EagerRefreshThreshold = 0.9f; // Would become 1.0 without cap
        _multiTenancyConfig.IsEnabled.Returns(true);
        _session.TenantId.Returns((int?)null);

        // Act
        var result = _modifier.ModifyOptions(originalOptions, cacheKey);

        // Assert
        await Assert.That(result.EagerRefreshThreshold).IsEqualTo(0.95f);
    }

    [Test]
    public async Task ModifyOptions_ForHost_WhenCacheStampedeProtectionEnabled_ShouldSetMinimalJitter()
    {
        // Arrange
        var originalOptions = new FusionCacheEntryOptions(TimeSpan.FromMinutes(30));
        var cacheKey = "testKey";

        _cacheOptions.EnableMultiTenancy = true;
        _cacheOptions.EnableCacheStampedeProtection = true;
        _multiTenancyConfig.IsEnabled.Returns(true);
        _session.TenantId.Returns((int?)null);

        // Act
        var result = _modifier.ModifyOptions(originalOptions, cacheKey);

        // Assert
        await Assert.That(result.JitterMaxDuration).IsEqualTo(TimeSpan.FromMilliseconds(25));
    }

    #endregion

    #region Options Not Modified Tests

    [Test]
    public async Task ModifyOptions_ShouldNotModifyOriginalOptions()
    {
        // Arrange
        var originalDuration = TimeSpan.FromMinutes(100);
        var originalOptions = new FusionCacheEntryOptions(originalDuration);
        var cacheKey = "testKey";
        var tenantId = 123;

        _cacheOptions.EnableMultiTenancy = true;
        _multiTenancyConfig.IsEnabled.Returns(true);
        _session.TenantId.Returns(tenantId);

        // Act
        var result = _modifier.ModifyOptions(originalOptions, cacheKey);

        // Assert - Original should remain unchanged
        await Assert.That(originalOptions.Duration).IsEqualTo(originalDuration);
        // Result should be different
        await Assert.That(result.Duration).IsNotEqualTo(originalOptions.Duration);
    }

    #endregion

    #region Edge Cases Tests

    [Test]
    public async Task ModifyOptions_ForTenant_WithVeryShortDuration_ShouldStillReduce()
    {
        // Arrange
        var originalDuration = TimeSpan.FromSeconds(4);
        var originalOptions = new FusionCacheEntryOptions(originalDuration);
        var cacheKey = "testKey";
        var tenantId = 123;

        _cacheOptions.EnableMultiTenancy = true;
        _multiTenancyConfig.IsEnabled.Returns(true);
        _session.TenantId.Returns(tenantId);

        // Act
        var result = _modifier.ModifyOptions(originalOptions, cacheKey);

        // Assert
        var expectedDuration = TimeSpan.FromSeconds(3); // 75% of 4 seconds
        await Assert.That(result.Duration).IsEqualTo(expectedDuration);
    }

    [Test]
    public async Task ModifyOptions_ForHost_WithVeryLongDuration_ShouldStillIncrease()
    {
        // Arrange
        var originalDuration = TimeSpan.FromHours(24);
        var originalOptions = new FusionCacheEntryOptions(originalDuration);
        var cacheKey = "testKey";

        _cacheOptions.EnableMultiTenancy = true;
        _multiTenancyConfig.IsEnabled.Returns(true);
        _session.TenantId.Returns((int?)null);

        // Act
        var result = _modifier.ModifyOptions(originalOptions, cacheKey);

        // Assert
        var expectedDuration = TimeSpan.FromHours(36); // 150% of 24 hours
        await Assert.That(result.Duration).IsEqualTo(expectedDuration);
    }

    [Test]
    public async Task ModifyOptions_ForTenant_WithJitterRange_ShouldBeInValidRange()
    {
        // Arrange
        var originalOptions = new FusionCacheEntryOptions(TimeSpan.FromMinutes(30));
        var cacheKey = "testKey";

        _cacheOptions.EnableMultiTenancy = true;
        _cacheOptions.EnableCacheStampedeProtection = true;
        _multiTenancyConfig.IsEnabled.Returns(true);

        // Test multiple tenant IDs
        for (int tenantId = 0; tenantId < 200; tenantId++)
        {
            _session.TenantId.Returns(tenantId);
            var result = _modifier.ModifyOptions(originalOptions, cacheKey);

            // Assert - Jitter should be between 50ms and 149ms
            var jitterMs = result.JitterMaxDuration.TotalMilliseconds;
            await Assert.That(jitterMs).IsGreaterThanOrEqualTo(50);
            await Assert.That(jitterMs).IsLessThanOrEqualTo(149);
        }
    }

    #endregion

    #region Integration Tests

    [Test]
    public async Task ModifyOptions_ForTenant_WithAllFeaturesEnabled_ShouldApplyAllModifications()
    {
        // Arrange
        var originalOptions = new FusionCacheEntryOptions(TimeSpan.FromMinutes(100));
        originalOptions.SetFailSafe(false);
        var cacheKey = "testKey";
        var tenantId = 42;

        _cacheOptions.EnableMultiTenancy = true;
        _cacheOptions.EnableEagerRefresh = true;
        _cacheOptions.EagerRefreshThreshold = 0.85f;
        _cacheOptions.EnableCacheStampedeProtection = true;
        _multiTenancyConfig.IsEnabled.Returns(true);
        _session.TenantId.Returns(tenantId);

        // Act
        var result = _modifier.ModifyOptions(originalOptions, cacheKey);

        // Assert - All modifications should be applied
        await Assert.That(result.Duration).IsEqualTo(TimeSpan.FromMinutes(75)); // 75% reduction
        await Assert.That(result.Priority).IsEqualTo(CacheItemPriority.High);
        await Assert.That(result.IsFailSafeEnabled).IsTrue();
        await Assert.That(result.FailSafeMaxDuration).IsEqualTo(TimeSpan.FromHours(1));
        await Assert.That(result.EagerRefreshThreshold).IsEqualTo(0.85f);
        await Assert.That(result.JitterMaxDuration).IsEqualTo(TimeSpan.FromMilliseconds(92)); // (42 % 100) + 50
    }

    [Test]
    public async Task ModifyOptions_ForHost_WithAllFeaturesEnabled_ShouldApplyAllModifications()
    {
        // Arrange
        var originalOptions = new FusionCacheEntryOptions(TimeSpan.FromMinutes(100));
        originalOptions.SetFailSafe(false);
        var cacheKey = "testKey";

        _cacheOptions.EnableMultiTenancy = true;
        _cacheOptions.EnableEagerRefresh = true;
        _cacheOptions.EagerRefreshThreshold = 0.8f;
        _cacheOptions.EnableCacheStampedeProtection = true;
        _multiTenancyConfig.IsEnabled.Returns(true);
        _session.TenantId.Returns((int?)null);

        // Act
        var result = _modifier.ModifyOptions(originalOptions, cacheKey);

        // Assert - All modifications should be applied
        await Assert.That(result.Duration).IsEqualTo(TimeSpan.FromMinutes(150)); // 150% increase
        await Assert.That(result.Priority).IsEqualTo(CacheItemPriority.Normal);
        await Assert.That(result.IsFailSafeEnabled).IsTrue();
        await Assert.That(result.FailSafeMaxDuration).IsEqualTo(TimeSpan.FromHours(4));
        await Assert.That(result.EagerRefreshThreshold).IsEqualTo(0.9f); // 0.8 + 0.1
        await Assert.That(result.JitterMaxDuration).IsEqualTo(TimeSpan.FromMilliseconds(25));
    }

    #endregion
}
