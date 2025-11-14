using CommunityAbp.AspNetZero.FusionCache.Internal;
using CommunityAbp.AspNetZero.FusionCache.Runtime.Caching.FusionCache;
using NSubstitute;
using ZiggyCreatures.Caching.Fusion;

namespace CommunityAbp.AspNetZero.FusionCache.Tests;

public class AbpFusionCacheTests
{
    private IFusionCache _fusionCache = null!;
    private IAbpFusionCacheKeyNormalizer _keyNormalizer = null!;
    private IAbpFusionCacheSerializer _serializer = null!;
    private IAbpMultiTenancyFusionCacheEntryOptionsModifier _optionsModifier = null!;
    private AbpFusionCache _cache = null!;
    private const string CacheName = "TestCache";

    [Before(Test)]
    public void Setup()
    {
        _fusionCache = Substitute.For<IFusionCache>();
        _keyNormalizer = Substitute.For<IAbpFusionCacheKeyNormalizer>();
        _serializer = Substitute.For<IAbpFusionCacheSerializer>();
        _optionsModifier = Substitute.For<IAbpMultiTenancyFusionCacheEntryOptionsModifier>();

        // Setup default FusionCache options
        var defaultOptions = new FusionCacheEntryOptions(TimeSpan.FromMinutes(30));
        _fusionCache.DefaultEntryOptions.Returns(defaultOptions);

        _cache = new TestAbpFusionCache(
            CacheName,
            _fusionCache,
            _keyNormalizer,
            _serializer,
            _optionsModifier
        );
    }

    #region Constructor Tests

    [Test]
    public async Task Constructor_ShouldSetCacheName()
    {
        await Assert.That(_cache.Name).IsEqualTo(CacheName);
    }

    #endregion

    #region GetOrDefault Tests

    [Test]
    public async Task GetOrDefault_WithExistingKey_ShouldReturnDeserializedValue()
    {
        // Arrange
        var key = "testKey";
        var normalizedKey = "normalized:testKey";
        var serializedValue = "{\"value\":\"test\"}";
        var expectedValue = "test";

        _keyNormalizer.NormalizeKey(key).Returns(normalizedKey);
        _fusionCache.TryGet<string>(normalizedKey).Returns(MaybeValue<string>.FromValue(serializedValue));
        _serializer.Deserialize(serializedValue).Returns(expectedValue);

        // Act
        var result = _cache.GetOrDefault(key);

        // Assert
        await Assert.That(result).IsEqualTo(expectedValue);
        _keyNormalizer.Received(1).NormalizeKey(key);
        _fusionCache.Received(1).TryGet<string>(normalizedKey);
        _serializer.Received(1).Deserialize(serializedValue);
    }

    [Test]
    public async Task GetOrDefault_WithNonExistingKey_ShouldReturnNull()
    {
        // Arrange
        var key = "nonExistingKey";
        var normalizedKey = "normalized:nonExistingKey";

        _keyNormalizer.NormalizeKey(key).Returns(normalizedKey);
        _fusionCache.TryGet<string>(normalizedKey).Returns(default(MaybeValue<string>));

        // Act
        var result = _cache.GetOrDefault(key);

        // Assert
        await Assert.That(result).IsNull();
        _keyNormalizer.Received(1).NormalizeKey(key);
        _fusionCache.Received(1).TryGet<string>(normalizedKey);
        _serializer.DidNotReceive().Deserialize(Arg.Any<string>());
    }

    #endregion

    #region TryGetValue Tests

    [Test]
    public async Task TryGetValue_WithExistingKey_ShouldReturnTrueAndValue()
    {
        // Arrange
        var key = "testKey";
        var normalizedKey = "normalized:testKey";
        var serializedValue = "{\"value\":\"test\"}";
        var expectedValue = "test";

        _keyNormalizer.NormalizeKey(key).Returns(normalizedKey);
        _fusionCache.TryGet<string>(normalizedKey).Returns(MaybeValue<string>.FromValue(serializedValue));
        _serializer.Deserialize(serializedValue).Returns(expectedValue);

        // Act
        var result = _cache.TryGetValue(key, out var value);

        // Assert
        await Assert.That(result).IsTrue();
        await Assert.That(value).IsEqualTo(expectedValue);
        _keyNormalizer.Received(1).NormalizeKey(key);
        _fusionCache.Received(1).TryGet<string>(normalizedKey);
        _serializer.Received(1).Deserialize(serializedValue);
    }

    [Test]
    public async Task TryGetValue_WithNonExistingKey_ShouldReturnFalseAndNull()
    {
        // Arrange
        var key = "nonExistingKey";
        var normalizedKey = "normalized:nonExistingKey";

        _keyNormalizer.NormalizeKey(key).Returns(normalizedKey);
        _fusionCache.TryGet<string>(normalizedKey).Returns(default(MaybeValue<string>));

        // Act
        var result = _cache.TryGetValue(key, out var value);

        // Assert
        await Assert.That(result).IsFalse();
        await Assert.That(value).IsNull();
        _serializer.DidNotReceive().Deserialize(Arg.Any<string>());
    }

    #endregion

    #region GetOrDefaultAsync Tests

    [Test]
    public async Task GetOrDefaultAsync_WithExistingKey_ShouldReturnDeserializedValue()
    {
        // Arrange
        var key = "testKey";
        var normalizedKey = "normalized:testKey";
        var serializedValue = "{\"value\":\"test\"}";
        var expectedValue = "test";

        _keyNormalizer.NormalizeKey(key).Returns(normalizedKey);
        _fusionCache.TryGetAsync<string>(normalizedKey).Returns(ValueTask.FromResult(MaybeValue<string>.FromValue(serializedValue)));
        _serializer.Deserialize(serializedValue).Returns(expectedValue);

        // Act
        var result = await _cache.GetOrDefaultAsync(key);

        // Assert
        await Assert.That(result).IsEqualTo(expectedValue);
        _keyNormalizer.Received(1).NormalizeKey(key);
        await _fusionCache.Received(1).TryGetAsync<string>(normalizedKey);
        _serializer.Received(1).Deserialize(serializedValue);
    }

    [Test]
    public async Task GetOrDefaultAsync_WithNonExistingKey_ShouldReturnNull()
    {
        // Arrange
        var key = "nonExistingKey";
        var normalizedKey = "normalized:nonExistingKey";

        _keyNormalizer.NormalizeKey(key).Returns(normalizedKey);
        _fusionCache.TryGetAsync<string>(normalizedKey).Returns(ValueTask.FromResult(default(MaybeValue<string>)));

        // Act
        var result = await _cache.GetOrDefaultAsync(key);

        // Assert
        await Assert.That(result).IsNull();
        _keyNormalizer.Received(1).NormalizeKey(key);
        await _fusionCache.Received(1).TryGetAsync<string>(normalizedKey);
        _serializer.DidNotReceive().Deserialize(Arg.Any<string>());
    }

    #endregion

    #region Set Tests

    [Test]
    public async Task Set_WithValue_ShouldSerializeAndStore()
    {
        // Arrange
        var key = "testKey";
        var normalizedKey = "normalized:testKey";
        var value = "testValue";
        var serializedValue = "{\"value\":\"testValue\"}";
        var modifiedOptions = new FusionCacheEntryOptions(TimeSpan.FromMinutes(30));

        _keyNormalizer.NormalizeKey(key).Returns(normalizedKey);
        _serializer.Serialize(value).Returns(serializedValue);
        _optionsModifier.ModifyOptions(Arg.Any<FusionCacheEntryOptions>(), normalizedKey).Returns(modifiedOptions);

        // Act
        _cache.Set(key, value);

        // Assert
        _keyNormalizer.Received(1).NormalizeKey(key);
        _serializer.Received(1).Serialize(value);
        _fusionCache.Received(1).Set(normalizedKey, serializedValue, modifiedOptions);
        _optionsModifier.Received(1).ModifyOptions(Arg.Any<FusionCacheEntryOptions>(), normalizedKey);
    }

    [Test]
    public async Task Set_WithSlidingExpiration_ShouldUseCorrectDuration()
    {
        // Arrange
        var key = "testKey";
        var normalizedKey = "normalized:testKey";
        var value = "testValue";
        var serializedValue = "{\"value\":\"testValue\"}";
        var slidingExpiration = TimeSpan.FromMinutes(10);
        FusionCacheEntryOptions? capturedOptions = null;

        _keyNormalizer.NormalizeKey(key).Returns(normalizedKey);
        _serializer.Serialize(value).Returns(serializedValue);
        _optionsModifier.ModifyOptions(Arg.Any<FusionCacheEntryOptions>(), normalizedKey)
            .Returns(callInfo =>
            {
                capturedOptions = callInfo.ArgAt<FusionCacheEntryOptions>(0);
                return capturedOptions;
            });

        // Act
        _cache.Set(key, value, slidingExpiration);

        // Assert
        await Assert.That(capturedOptions).IsNotNull();
        await Assert.That(capturedOptions!.Duration).IsEqualTo(slidingExpiration);
    }

    [Test]
    public async Task Set_WithAbsoluteExpiration_ShouldUseCorrectDuration()
    {
        // Arrange
        var key = "testKey";
        var normalizedKey = "normalized:testKey";
        var value = "testValue";
        var serializedValue = "{\"value\":\"testValue\"}";
        var absoluteExpiration = DateTimeOffset.Now.AddMinutes(15);
        FusionCacheEntryOptions? capturedOptions = null;

        _keyNormalizer.NormalizeKey(key).Returns(normalizedKey);
        _serializer.Serialize(value).Returns(serializedValue);
        _optionsModifier.ModifyOptions(Arg.Any<FusionCacheEntryOptions>(), normalizedKey)
            .Returns(callInfo =>
            {
                capturedOptions = callInfo.ArgAt<FusionCacheEntryOptions>(0);
                return capturedOptions;
            });

        // Act
        _cache.Set(key, value, absoluteExpireTime: absoluteExpiration);

        // Assert
        await Assert.That(capturedOptions).IsNotNull();
        // Duration should be approximately 15 minutes (allowing for small time passage)
        await Assert.That(capturedOptions!.Duration).IsGreaterThan(TimeSpan.FromMinutes(14));
        await Assert.That(capturedOptions!.Duration).IsLessThanOrEqualTo(TimeSpan.FromMinutes(15));
    }

    [Test]
    public async Task Set_WithExpiredAbsoluteExpiration_ShouldUseMinimumDuration()
    {
        // Arrange
        var key = "testKey";
        var normalizedKey = "normalized:testKey";
        var value = "testValue";
        var serializedValue = "{\"value\":\"testValue\"}";
        var absoluteExpiration = DateTimeOffset.Now.AddMinutes(-5); // Already expired
        FusionCacheEntryOptions? capturedOptions = null;

        _keyNormalizer.NormalizeKey(key).Returns(normalizedKey);
        _serializer.Serialize(value).Returns(serializedValue);
        _optionsModifier.ModifyOptions(Arg.Any<FusionCacheEntryOptions>(), normalizedKey)
            .Returns(callInfo =>
            {
                capturedOptions = callInfo.ArgAt<FusionCacheEntryOptions>(0);
                return capturedOptions;
            });

        // Act
        _cache.Set(key, value, absoluteExpireTime: absoluteExpiration);

        // Assert
        await Assert.That(capturedOptions).IsNotNull();
        await Assert.That(capturedOptions!.Duration).IsEqualTo(TimeSpan.FromSeconds(1));
    }

    #endregion

    #region SetAsync Tests

    [Test]
    public async Task SetAsync_WithValue_ShouldSerializeAndStore()
    {
        // Arrange
        var key = "testKey";
        var normalizedKey = "normalized:testKey";
        var value = "testValue";
        var serializedValue = "{\"value\":\"testValue\"}";
        var modifiedOptions = new FusionCacheEntryOptions(TimeSpan.FromMinutes(30));

        _keyNormalizer.NormalizeKey(key).Returns(normalizedKey);
        _serializer.Serialize(value).Returns(serializedValue);
        _optionsModifier.ModifyOptions(Arg.Any<FusionCacheEntryOptions>(), normalizedKey).Returns(modifiedOptions);

        // Act
        await _cache.SetAsync(key, value);

        // Assert
        _keyNormalizer.Received(1).NormalizeKey(key);
        _serializer.Received(1).Serialize(value);
        await _fusionCache.Received(1).SetAsync(normalizedKey, serializedValue, modifiedOptions);
        _optionsModifier.Received(1).ModifyOptions(Arg.Any<FusionCacheEntryOptions>(), normalizedKey);
    }

    [Test]
    public async Task SetAsync_WithSlidingExpiration_ShouldUseCorrectDuration()
    {
        // Arrange
        var key = "testKey";
        var normalizedKey = "normalized:testKey";
        var value = "testValue";
        var serializedValue = "{\"value\":\"testValue\"}";
        var slidingExpiration = TimeSpan.FromMinutes(10);
        FusionCacheEntryOptions? capturedOptions = null;

        _keyNormalizer.NormalizeKey(key).Returns(normalizedKey);
        _serializer.Serialize(value).Returns(serializedValue);
        _optionsModifier.ModifyOptions(Arg.Any<FusionCacheEntryOptions>(), normalizedKey)
            .Returns(callInfo =>
            {
                capturedOptions = callInfo.ArgAt<FusionCacheEntryOptions>(0);
                return capturedOptions;
            });

        // Act
        await _cache.SetAsync(key, value, slidingExpiration);

        // Assert
        await Assert.That(capturedOptions).IsNotNull();
        await Assert.That(capturedOptions!.Duration).IsEqualTo(slidingExpiration);
    }

    #endregion

    #region Remove Tests

    [Test]
    public async Task Remove_ShouldNormalizeKeyAndRemoveFromCache()
    {
        // Arrange
        var key = "testKey";
        var normalizedKey = "normalized:testKey";

        _keyNormalizer.NormalizeKey(key).Returns(normalizedKey);

        // Act
        _cache.Remove(key);

        // Assert
        _keyNormalizer.Received(1).NormalizeKey(key);
        _fusionCache.Received(1).Remove(normalizedKey);
    }

    #endregion

    #region RemoveAsync Tests

    [Test]
    public async Task RemoveAsync_ShouldNormalizeKeyAndRemoveFromCache()
    {
        // Arrange
        var key = "testKey";
        var normalizedKey = "normalized:testKey";

        _keyNormalizer.NormalizeKey(key).Returns(normalizedKey);

        // Act
        await _cache.RemoveAsync(key);

        // Assert
        _keyNormalizer.Received(1).NormalizeKey(key);
        await _fusionCache.Received(1).RemoveAsync(normalizedKey);
    }

    #endregion

    #region Clear Tests

    [Test]
    public async Task Clear_ShouldCallFusionCacheClear()
    {
        // Act
        _cache.Clear();

        // Assert
        _fusionCache.Received(1).Clear();
    }

    [Test]
    public async Task Clear_WhenNotSupported_ShouldThrowNotSupportedException()
    {
        // Arrange
        _fusionCache.When(x => x.Clear()).Do(_ => throw new NotSupportedException("Clear operation is not supported"));

        // Act & Assert
        var ex = await Assert.That(() => Task.Run(() => _cache.Clear())).Throws<NotSupportedException>();
        await Assert.That(ex?.Message).Contains("Clear operation is not supported");
    }

    #endregion

    #region ClearAsync Tests

    [Test]
    public async Task ClearAsync_ShouldCallFusionCacheClearAsync()
    {
        // Act
        await _cache.ClearAsync();

        // Assert
        await _fusionCache.Received(1).ClearAsync();
    }

    [Test]
    public async Task ClearAsync_WhenNotSupported_ShouldThrowNotSupportedException()
    {
        // Arrange
        _fusionCache.ClearAsync().Returns(ValueTask.FromException(new NotSupportedException("Clear operation is not supported")));

        // Act & Assert
        var ex = await Assert.That(async () => await _cache.ClearAsync()).Throws<NotSupportedException>();
        await Assert.That(ex?.Message).Contains("Clear operation is not supported");
    }

    #endregion

    #region DefaultSlidingExpireTime Tests

    [Test]
    public async Task Set_WithNoExpiration_ShouldUseDefaultSlidingExpireTime()
    {
        // Arrange
        var key = "testKey";
        var normalizedKey = "normalized:testKey";
        var value = "testValue";
        var serializedValue = "{\"value\":\"testValue\"}";
        FusionCacheEntryOptions? capturedOptions = null;

        _keyNormalizer.NormalizeKey(key).Returns(normalizedKey);
        _serializer.Serialize(value).Returns(serializedValue);
        _optionsModifier.ModifyOptions(Arg.Any<FusionCacheEntryOptions>(), normalizedKey)
            .Returns(callInfo =>
            {
                capturedOptions = callInfo.ArgAt<FusionCacheEntryOptions>(0);
                return capturedOptions;
            });

        // Act
        _cache.Set(key, value);

        // Assert
        await Assert.That(capturedOptions).IsNotNull();
        // Should use the DefaultSlidingExpireTime from CacheBase
        await Assert.That(capturedOptions!.Duration).IsEqualTo(_cache.DefaultSlidingExpireTime);
    }

    #endregion
}
