using Microsoft.EntityFrameworkCore;
using ProductCatalog.Infrastructure.EntityFramework;
using ProductCatalog.Shared.Entities;

namespace ProductCatalog.Infrastructure;

/// <summary>
/// Seeds the database with sample product data for testing.
/// </summary>
public static class SampleDataSeeder
{
    public static async Task SeedAsync(ProductDbContext context)
    {
        // Check if data already exists
        if (await context.Products.AnyAsync())
        {
            return; // Database already seeded
        }

        var products = new List<Product>
        {
            new()
            {
                Name = "Laptop Pro 15\"",
                Description = "High-performance laptop with 16GB RAM and 512GB SSD",
                Sku = "LAPTOP-PRO-15",
                Price = 1299.99m,
                StockQuantity = 25,
                IsActive = true,
                Category = "Electronics"
            },
            new()
            {
                Name = "Wireless Mouse",
                Description = "Ergonomic wireless mouse with 6 programmable buttons",
                Sku = "MOUSE-WL-001",
                Price = 29.99m,
                StockQuantity = 150,
                IsActive = true,
                Category = "Electronics"
            },
            new()
            {
                Name = "Mechanical Keyboard",
                Description = "RGB mechanical keyboard with Cherry MX Blue switches",
                Sku = "KB-MECH-RGB",
                Price = 89.99m,
                StockQuantity = 75,
                IsActive = true,
                Category = "Electronics"
            },
            new()
            {
                Name = "USB-C Hub",
                Description = "7-in-1 USB-C hub with HDMI, USB 3.0, and SD card reader",
                Sku = "HUB-USBC-7IN1",
                Price = 39.99m,
                StockQuantity = 200,
                IsActive = true,
                Category = "Accessories"
            },
            new()
            {
                Name = "Webcam HD 1080p",
                Description = "Full HD webcam with built-in microphone and auto-focus",
                Sku = "WEBCAM-HD-1080",
                Price = 69.99m,
                StockQuantity = 50,
                IsActive = true,
                Category = "Electronics"
            },
            new()
            {
                Name = "Laptop Stand",
                Description = "Adjustable aluminum laptop stand for better ergonomics",
                Sku = "STAND-LAP-ADJ",
                Price = 34.99m,
                StockQuantity = 100,
                IsActive = true,
                Category = "Accessories"
            },
            new()
            {
                Name = "Noise-Cancelling Headphones",
                Description = "Premium over-ear headphones with active noise cancellation",
                Sku = "HEADPHONE-NC-001",
                Price = 249.99m,
                StockQuantity = 40,
                IsActive = true,
                Category = "Audio"
            },
            new()
            {
                Name = "Portable SSD 1TB",
                Description = "Ultra-fast portable SSD with USB 3.2 Gen 2",
                Sku = "SSD-PORT-1TB",
                Price = 119.99m,
                StockQuantity = 80,
                IsActive = true,
                Category = "Storage"
            },
            new()
            {
                Name = "Monitor 27\" 4K",
                Description = "27-inch 4K UHD monitor with IPS panel and HDR support",
                Sku = "MON-27-4K",
                Price = 399.99m,
                StockQuantity = 30,
                IsActive = true,
                Category = "Electronics"
            },
            new()
            {
                Name = "Desk Lamp LED",
                Description = "Adjustable LED desk lamp with touch control and USB charging",
                Sku = "LAMP-DESK-LED",
                Price = 44.99m,
                StockQuantity = 120,
                IsActive = true,
                Category = "Accessories"
            },
            new()
            {
                Name = "Cable Management Kit",
                Description = "Complete cable management solution for clean desk setup",
                Sku = "CABLE-MGT-KIT",
                Price = 19.99m,
                StockQuantity = 250,
                IsActive = true,
                Category = "Accessories"
            },
            new()
            {
                Name = "Smartphone Stand",
                Description = "Universal adjustable smartphone stand for desk",
                Sku = "STAND-PHONE-001",
                Price = 14.99m,
                StockQuantity = 180,
                IsActive = true,
                Category = "Accessories"
            },
            new()
            {
                Name = "External Battery 20000mAh",
                Description = "High-capacity power bank with fast charging",
                Sku = "BATTERY-EXT-20K",
                Price = 49.99m,
                StockQuantity = 90,
                IsActive = true,
                Category = "Accessories"
            },
            new()
            {
                Name = "Bluetooth Speaker",
                Description = "Portable Bluetooth speaker with 360-degree sound",
                Sku = "SPEAKER-BT-360",
                Price = 79.99m,
                StockQuantity = 60,
                IsActive = true,
                Category = "Audio"
            },
            new()
            {
                Name = "Graphics Tablet",
                Description = "Digital drawing tablet with 8192 pressure levels",
                Sku = "TABLET-GRAPH-001",
                Price = 159.99m,
                StockQuantity = 35,
                IsActive = true,
                Category = "Electronics"
            },
            new()
            {
                Name = "Docking Station",
                Description = "Universal laptop docking station with dual monitor support",
                Sku = "DOCK-UNIV-DUAL",
                Price = 199.99m,
                StockQuantity = 25,
                IsActive = true,
                Category = "Accessories"
            },
            new()
            {
                Name = "Microphone USB",
                Description = "Professional USB condenser microphone for streaming",
                Sku = "MIC-USB-PRO",
                Price = 99.99m,
                StockQuantity = 45,
                IsActive = true,
                Category = "Audio"
            },
            new()
            {
                Name = "Ergonomic Chair Mat",
                Description = "Premium chair mat for hardwood and carpet floors",
                Sku = "MAT-CHAIR-ERG",
                Price = 64.99m,
                StockQuantity = 55,
                IsActive = false, // Intentionally inactive for testing
                Category = "Furniture"
            }
        };

        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();
    }
}
