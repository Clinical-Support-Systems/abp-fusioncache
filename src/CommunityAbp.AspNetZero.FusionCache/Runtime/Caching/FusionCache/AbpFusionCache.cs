using Abp.Runtime.Caching;
using CommunityAbp.AspNetZero.FusionCache.Internal;
using ZiggyCreatures.Caching.Fusion;

namespace CommunityAbp.AspNetZero.FusionCache.Runtime.Caching.FusionCache
{
    /// <summary>
    /// Provides an implementation of a distributed cache using FusionCache, supporting key normalization,
    /// serialization, and multi-tenancy features.
    /// </summary>
    /// <remarks>AbpFusionCache integrates FusionCache with ABP's caching abstractions, enabling advanced
    /// caching scenarios such as sliding and absolute expiration, multi-tenancy key isolation, and custom
    /// serialization. This class is typically used internally by the ABP framework to provide a consistent caching
    /// experience across different cache providers. Thread safety and performance characteristics depend on the
    /// underlying FusionCache implementation.</remarks>
    public class AbpFusionCache : CacheBase
    {
        private readonly IFusionCache _fusionCache;
        private readonly IAbpFusionCacheKeyNormalizer _keyNormalizer;
        private readonly IAbpFusionCacheSerializer _serializer;
        private readonly IAbpMultiTenancyFusionCacheEntryOptionsModifier _optionsModifier;

        /// <summary>
        /// Initializes a new instance of the AbpFusionCache class with the specified cache name, underlying FusionCache
        /// instance, key normalizer, serializer, and multi-tenancy configuration.
        /// </summary>
        /// <param name="name">The unique name used to identify this cache instance.</param>
        /// <param name="fusionCache">The underlying FusionCache instance used for caching operations.</param>
        /// <param name="keyNormalizer">The key normalizer responsible for standardizing cache keys.</param>
        /// <param name="serializer">The serializer used to convert objects to and from a format suitable for caching.</param>
        /// scenarios.</param>
        public AbpFusionCache(
            string name,
            IFusionCache fusionCache,
            IAbpFusionCacheKeyNormalizer keyNormalizer,
            IAbpFusionCacheSerializer serializer,
            IAbpMultiTenancyFusionCacheEntryOptionsModifier optionsModifier)
            : base(name)
        {
            _fusionCache = fusionCache;
            _keyNormalizer = keyNormalizer;
            _serializer = serializer;
            _optionsModifier = optionsModifier;
        }

        // <summary>
        /// Creates FusionCacheEntryOptions based on FusionCache defaults and applies multi-tenancy modifications.
        /// </summary>
        private FusionCacheEntryOptions CreateAndModifyOptions(string normalizedKey, TimeSpan duration)
        {
            // Start with a copy of FusionCache's default options
            var baseOptions = _fusionCache.DefaultEntryOptions.Duplicate(duration);

            // Apply multi-tenancy modifications
            var modifiedOptions = _optionsModifier.ModifyOptions(baseOptions, normalizedKey);

            return modifiedOptions;
        }

        /// <summary>
        /// Retrieves the value associated with the specified key, or returns null if the key does not exist.
        /// </summary>
        /// <param name="key">The key whose associated value is to be retrieved. Cannot be null.</param>
        /// <returns>The deserialized value associated with the specified key, or null if the key is not found.</returns>
        public override object GetOrDefault(string key)
        {
            var normalizedKey = _keyNormalizer.NormalizeKey(key);
            var result = _fusionCache.TryGet<string>(normalizedKey);

            if (result.HasValue)
            {
                return _serializer.Deserialize(result.Value)!;
            }

            return null!;
        }

        /// <summary>
        /// Attempts to retrieve the value associated with the specified key from the cache.
        /// </summary>
        /// <remarks>The key is normalized before lookup. If the key is not found, the out parameter is
        /// set to null.</remarks>
        /// <param name="key">The key whose value to retrieve. Cannot be null.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found;
        /// otherwise, the default value for the type of the value parameter.</param>
        /// <returns>true if the cache contains an entry with the specified key; otherwise, false.</returns>
        public override bool TryGetValue(string key, out object value)
        {
            var normalizedKey = _keyNormalizer.NormalizeKey(key);
            var result = _fusionCache.TryGet<string>(normalizedKey);
            if (result.HasValue)
            {
                value = _serializer.Deserialize(result.Value)!;
                return true;
            }
            value = null!;
            return false;
        }

        /// <summary>
        /// Asynchronously retrieves the value associated with the specified key, or returns null if the key does not
        /// exist.
        /// </summary>
        /// <param name="key">The key whose associated value is to be retrieved. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the deserialized value
        /// associated with the specified key, or null if the key is not found.</returns>
        public override async Task<object> GetOrDefaultAsync(string key)
        {
            var normalizedKey = _keyNormalizer.NormalizeKey(key);
            var result = await _fusionCache.TryGetAsync<string>(normalizedKey);

            if (result.HasValue)
            {
                return _serializer.Deserialize(result.Value)!;
            }

            return null!;
        }

        /// <summary>
        /// Sets a value in the cache with the specified key and optional expiration settings, overwriting any existing
        /// value for the key.
        /// </summary>
        /// <remarks>If both sliding and absolute expiration are provided, the cache entry will expire
        /// when the first of the two conditions is met. Providing neither will result in the cache entry using the
        /// default expiration policy, if any.</remarks>
        /// <param name="key">The unique identifier for the cache entry. Cannot be null or empty.</param>
        /// <param name="value">The value to store in the cache. Cannot be null.</param>
        /// <param name="slidingExpireTime">An optional sliding expiration time. If specified, resets the expiration timer each time the cache entry is
        /// accessed.</param>
        /// <param name="absoluteExpireTime">An optional absolute expiration date and time. If specified, the cache entry expires at this point
        /// regardless of access.</param>
        public override void Set(string key, object value, TimeSpan? slidingExpireTime = null, DateTimeOffset? absoluteExpireTime = null)
        {
            var normalizedKey = _keyNormalizer.NormalizeKey(key);
            var serializedValue = _serializer.Serialize(value);

            var duration = CalculateDuration(slidingExpireTime, absoluteExpireTime);

            // Create base options and apply multi-tenancy modifications
            var options = CreateAndModifyOptions(normalizedKey, duration);

            _fusionCache.Set(normalizedKey, serializedValue, options);
        }

        /// <summary>
        /// Asynchronously sets a value in the cache with the specified key and optional expiration settings.
        /// </summary>
        /// <param name="key">The unique identifier for the cache entry. Cannot be null.</param>
        /// <param name="value">The value to store in the cache. Cannot be null.</param>
        /// <param name="slidingExpireTime">An optional sliding expiration time. If specified, resets the expiration timer each time the cache entry is
        /// accessed.</param>
        /// <param name="absoluteExpireTime">An optional absolute expiration date and time for the cache entry. If specified, the entry expires at this
        /// time regardless of access.</param>
        /// <returns>A task that represents the asynchronous set operation.</returns>
        public override async Task SetAsync(string key, object value, TimeSpan? slidingExpireTime = null, DateTimeOffset? absoluteExpireTime = null)
        {
            var normalizedKey = _keyNormalizer.NormalizeKey(key);
            var serializedValue = _serializer.Serialize(value);

            var duration = CalculateDuration(slidingExpireTime, absoluteExpireTime);

            // Create base options and apply multi-tenancy modifications
            var options = CreateAndModifyOptions(normalizedKey, duration);

            await _fusionCache.SetAsync(normalizedKey, serializedValue, options);
        }

        /// <summary>
        /// Removes the cache entry associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the cache entry to remove. Cannot be null.</param>
        public override void Remove(string key)
        {
            var normalizedKey = _keyNormalizer.NormalizeKey(key);
            _fusionCache.Remove(normalizedKey);
        }

        /// <summary>
        /// Asynchronously removes the cache entry associated with the specified key.
        /// </summary>
        /// <param name="key">The key that identifies the cache entry to remove. Cannot be null or empty.</param>
        /// <returns>A task that represents the asynchronous remove operation.</returns>
        public override async Task RemoveAsync(string key)
        {
            var normalizedKey = _keyNormalizer.NormalizeKey(key);
            await _fusionCache.RemoveAsync(normalizedKey);
        }

        /// <summary>
        /// Removes all items from the cache.
        /// </summary>
        /// <remarks>Some FusionCache configurations may not implement support for clearing all cached
        /// items. In such cases, calling this method will throw a NotSupportedException.</remarks>
        /// <exception cref="NotSupportedException">Thrown if the underlying FusionCache configuration does not support clearing the cache.</exception>
        public override void Clear()
        {
            try
            {
                _fusionCache.Clear();
            }
            catch (NotSupportedException)
            {
                // Some FusionCache configurations may not support Clear
                throw new NotSupportedException("Clear operation is not supported by the current FusionCache configuration.");
            }
        }

        /// <summary>
        /// Asynchronously removes all items from the cache.
        /// </summary>
        /// <remarks>Some cache backends or configurations may not implement support for clearing all
        /// items. In such cases, this method will throw a NotSupportedException.</remarks>
        /// <returns>A task that represents the asynchronous clear operation.</returns>
        /// <exception cref="NotSupportedException">Thrown if the underlying cache configuration does not support clearing all items.</exception>
        public override async Task ClearAsync()
        {
            try
            {
                await _fusionCache.ClearAsync();
            }
            catch (NotSupportedException)
            {
                // Some FusionCache configurations may not support Clear
                throw new NotSupportedException("Clear operation is not supported by the current FusionCache configuration.");
            }
        }

        private TimeSpan CalculateDuration(TimeSpan? slidingExpireTime, DateTimeOffset? absoluteExpireTime)
        {
            if (slidingExpireTime.HasValue)
                return slidingExpireTime.Value;

            if (absoluteExpireTime.HasValue)
            {
                var remaining = absoluteExpireTime.Value - DateTimeOffset.Now;
                return remaining > TimeSpan.Zero ? remaining : TimeSpan.FromSeconds(1); // Minimum 1 second
            }

            // DefaultSlidingExpireTime is not nullable in CacheBase, so this is safe
            return DefaultSlidingExpireTime;
        }
    }
}
