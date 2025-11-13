using Abp.Dependency;
using CommunityAbp.AspNetZero.FusionCache.Options;

namespace CommunityAbp.AspNetZero.FusionCache.Configuration
{
    public class AbpFusionCacheConfiguration : IAbpFusionCacheConfiguration, ISingletonDependency
    {
        public AbpFusionCacheOptions Options { get; }

        public AbpFusionCacheConfiguration()
        {
            Options = new AbpFusionCacheOptions();
        }

        public void ConfigureFusionCache()
        {
            // Implementation will be added when we build the actual cache manager
        }
    }
}
