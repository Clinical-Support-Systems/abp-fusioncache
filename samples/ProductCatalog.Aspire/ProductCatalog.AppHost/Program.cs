var builder = DistributedApplication.CreateBuilder(args);

// Add Redis for distributed caching and backplane
var redis = builder.AddRedis("cache")
    .WithRedisCommander(); // Adds Redis Commander UI for cache inspection

// Add PostgreSQL database
var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin() // Adds pgAdmin UI for database management
    .AddDatabase("productdb");

// Add Public API service (read-heavy)
var api = builder.AddProject<Projects.ProductCatalog_Api>("api")
    .WithReference(redis)
    .WithReference(postgres)
    .WaitFor(redis)
    .WaitFor(postgres);

// Add Admin Portal service (write-heavy)
var admin = builder.AddProject<Projects.ProductCatalog_Admin>("admin")
    .WithReference(redis)
    .WithReference(postgres)
    .WaitFor(redis)
    .WaitFor(postgres);

await builder.Build().RunAsync();
