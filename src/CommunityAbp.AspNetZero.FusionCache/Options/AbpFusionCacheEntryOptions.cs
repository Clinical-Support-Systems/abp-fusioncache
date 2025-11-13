using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZiggyCreatures.Caching.Fusion;

namespace CommunityAbp.AspNetZero.FusionCache.Options
{
    public class AbpFusionCacheEntryOptions
    {
        public TimeSpan Duration { get; set; }
        public bool AllowBackgroundDistributedCacheOperations { get; set; } = true;
        public bool IsFailSafeEnabled { get; set; } = true;

        public FusionCacheEntryOptions ToFusionCacheEntryOptions()
        {
            return new FusionCacheEntryOptions()
                .SetDuration(Duration)
                .SetFailSafe(IsFailSafeEnabled);
        }
    }
}
