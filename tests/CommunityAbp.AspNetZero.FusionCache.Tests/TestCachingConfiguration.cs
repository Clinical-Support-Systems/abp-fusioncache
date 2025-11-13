using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Abp.Configuration.Startup;
using Abp.Runtime.Caching;
using Abp.Runtime.Caching.Configuration;
using Microsoft.Extensions.Caching.Memory;

namespace CommunityAbp.AspNetZero.FusionCache.Tests
{
    public class TestCachingConfiguration : ICachingConfiguration
    {
        public IAbpStartupConfiguration AbpConfiguration { get; set; } = null!;
        public IReadOnlyList<ICacheConfigurator> Configurators => _configurators.ToImmutableList();
        public MemoryCacheOptions MemoryCacheOptions { get; set; } = new MemoryCacheOptions();
        private readonly List<ICacheConfigurator> _configurators = new();

        public void ConfigureAll(Action<ICacheOptions> initAction)
        {
            _configurators.Add(new TestCacheConfigurator(initAction));
        }

        public void Configure(string cacheName, Action<ICacheOptions> initAction)
        {
            _configurators.Add(new TestCacheConfigurator(cacheName, initAction));
        }
    }

    public class TestCacheConfigurator : ICacheConfigurator
    {
        public string? CacheName { get; }
        public Action<ICacheOptions> InitAction { get; }
        public TestCacheConfigurator(Action<ICacheOptions> initAction)
        {
            InitAction = initAction;
        }
        public TestCacheConfigurator(string cacheName, Action<ICacheOptions> initAction)
        {
            CacheName = cacheName;
            InitAction = initAction;
        }
    }
}
