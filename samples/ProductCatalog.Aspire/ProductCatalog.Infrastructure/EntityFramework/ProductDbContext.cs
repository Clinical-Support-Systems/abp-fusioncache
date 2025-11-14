using Abp.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProductCatalog.Shared.Entities;

namespace ProductCatalog.Infrastructure.EntityFramework;

/// <summary>
/// DbContext for ProductCatalog application.
/// </summary>
public class ProductDbContext : AbpDbContext
{
    public DbSet<Product> Products { get; set; } = null!;

    public ProductDbContext(DbContextOptions<ProductDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(b =>
        {
            b.ToTable("Products");

            b.HasKey(p => p.Id);

            b.Property(p => p.Name).IsRequired().HasMaxLength(200);
            b.Property(p => p.Description).HasMaxLength(1000);
            b.Property(p => p.Sku).IsRequired().HasMaxLength(50);
            b.Property(p => p.Price).HasPrecision(18, 2);
            b.Property(p => p.Category).HasMaxLength(100);

            // Index for better query performance
            b.HasIndex(p => p.Sku);
            b.HasIndex(p => p.TenantId);
            b.HasIndex(p => new { p.TenantId, p.IsActive });
        });
    }
}
