using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Dependency;

namespace CommunityAbp.AspNetZero.FusionCache.Tests
{
    public abstract class TestBaseWithLocalIocManager : IDisposable
    {
        protected IIocManager LocalIocManager;

        protected TestBaseWithLocalIocManager()
        {
            LocalIocManager = new IocManager();
        }

        public virtual void Dispose()
        {
            LocalIocManager.Dispose();
        }
    }
}
