using Abp.Modules;
using Abp.Reflection.Extensions;

namespace ProductCatalog.Shared;

/// <summary>
/// Shared module containing entities and DTOs for ProductCatalog.
/// </summary>
[DependsOn(typeof(AbpKernelModule))]
public class ProductCatalogSharedModule : AbpModule
{
    public override void PreInitialize()
    {
        // Configure multi-tenancy
        Configuration.MultiTenancy.IsEnabled = true;
    }

    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(ProductCatalogSharedModule).GetAssembly());
    }
}
