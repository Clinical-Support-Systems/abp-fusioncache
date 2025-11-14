# Quick Start Guide

Get up and running with the ProductCatalog sample in 5 minutes!

## Prerequisites

- [x] .NET 9.0 SDK ([Download](https://dotnet.microsoft.com/download))
- [x] Docker Desktop ([Download](https://www.docker.com/products/docker-desktop))

## Steps

### 1. Start Docker Desktop

Make sure Docker Desktop is running on your machine.

### 2. Clone & Navigate

```bash
git clone https://github.com/Clinical-Support-Systems/abp-fusioncache.git
cd abp-fusioncache/samples/ProductCatalog.Aspire
```

### 3. Run the Application

```bash
cd ProductCatalog.AppHost
dotnet run
```

That's it! The Aspire dashboard will automatically open at `http://localhost:15000`

### 4. Explore

Open these URLs in your browser:

- **📊 Aspire Dashboard**: `http://localhost:15000` - Monitor everything
- **🔌 API (Swagger)**: `http://localhost:5100` - Test the API
- **⚙️ Admin Portal**: `http://localhost:5200` - Manage products

### 5. Test Cache Synchronization

**Try this:**

1. **Create a product** in the Admin Portal (`http://localhost:5200`)
   - Click "Create New Product"
   - Fill in the form and submit

2. **View it in the API** (`http://localhost:5100`)
   - Try `GET /api/products` endpoint
   - The product appears (cached)

3. **Edit the product** in Admin Portal
   - Change the name or price
   - Click "Update Product"

4. **Refresh the API** endpoint
   - The changes appear immediately!
   - This is the Redis backplane synchronizing caches ✨

### 6. View Logs

In the Aspire dashboard:
- Click "Logs" tab
- Filter by service (API or Admin)
- Watch cache hits/misses in real-time

## What's Happening Under the Hood?

```
Admin Updates Product
      ↓
Database Written
      ↓
Cache Invalidated (Redis)
      ↓
Redis Backplane Broadcasts
      ↓
API Receives Notification
      ↓
API's L1 Cache Auto-Syncs
      ↓
Next API Request Gets Fresh Data
```

## Common Issues

### Port Already in Use

If ports 5100, 5200, or 6379 are in use, you can change them in:
- `ProductCatalog.Api/Properties/launchSettings.json`
- `ProductCatalog.Admin/Properties/launchSettings.json`

### Docker Not Running

Error: `Cannot connect to Docker daemon`

**Solution:** Start Docker Desktop and try again.

### Redis Connection Failed

**Solution:**
- Check Aspire dashboard for Redis container status
- Click "Restart" if the container is stopped

## Next Steps

Read the full [README.md](./README.md) to learn about:
- Architecture details
- Testing scenarios
- Multi-tenancy
- Fail-safe mode
- Cache stampede protection

## Need Help?

Open an issue: https://github.com/Clinical-Support-Systems/abp-fusioncache/issues

Happy caching! 🚀
