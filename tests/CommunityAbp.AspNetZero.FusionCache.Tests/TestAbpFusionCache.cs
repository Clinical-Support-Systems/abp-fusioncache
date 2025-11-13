using CommunityAbp.AspNetZero.FusionCache.Runtime.Caching.FusionCache;
using ZiggyCreatures.Caching.Fusion;

namespace CommunityAbp.AspNetZero.FusionCache.Tests;

public class TestAbpFusionCache : AbpFusionCache
{
    public TestAbpFusionCache(
        string name,
        IFusionCache fusionCache,
        IAbpFusionCacheKeyNormalizer keyNormalizer,
        IAbpFusionCacheSerializer serializer,
        Internal.IAbpMultiTenancyFusionCacheEntryOptionsModifier optionsModifier)
        : base(name, fusionCache, keyNormalizer, serializer, optionsModifier)
    {
    }
}
