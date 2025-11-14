using Abp.EntityFrameworkCore;
using Abp.EntityFrameworkCore.Configuration;
using Abp.Modules;
using Abp.Reflection.Extensions;
using ProductCatalog.Infrastructure.EntityFramework;
using ProductCatalog.Shared;

namespace ProductCatalog.Infrastructure;

/// <summary>
/// Infrastructure module providing data access via Entity Framework Core.
/// </summary>
[DependsOn(
    typeof(ProductCatalogSharedModule),
    typeof(AbpEntityFrameworkCoreModule))]
public class ProductCatalogInfrastructureModule : AbpModule
{
    public override void PreInitialize()
    {
        Configuration.Modules.AbpEfCore().AddDbContext<ProductDbContext>(options =>
        {
            // Register default repositories
            options.DbContextOptions.UseNpgsql(options.ConnectionString);
        });
    }

    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(ProductCatalogInfrastructureModule).GetAssembly());
    }
}
