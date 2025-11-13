namespace CommunityAbp.AspNetZero.FusionCache.Runtime.Caching.FusionCache
{
    /// <summary>
    /// Defines a method that normalizes cache keys for use with FusionCache.
    /// </summary>
    /// <remarks>Implementations of this interface can apply transformations such as trimming, case
    /// normalization, or prefixing to ensure cache keys are consistent and compatible with FusionCache requirements.
    /// Normalizing keys helps prevent cache misses due to formatting differences.</remarks>
    public interface IAbpFusionCacheKeyNormalizer
    {
        /// <summary>
        /// Returns a normalized representation of the specified key for consistent comparison or storage.
        /// </summary>
        /// <param name="key">The key to normalize. Cannot be null.</param>
        /// <returns>A normalized string that represents the input key. The returned value is suitable for consistent use in
        /// lookups or comparisons.</returns>
        string NormalizeKey(string key);
    }
}
