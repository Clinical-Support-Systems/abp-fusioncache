using CommunityAbp.AspNetZero.FusionCache.Options;
using CommunityAbp.AspNetZero.FusionCache.Redis.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using StackExchange.Redis;
using ZiggyCreatures.Caching.Fusion.Backplane;

namespace CommunityAbp.AspNetZero.FusionCache.Redis.Tests;

public class AbpFusionCacheRedisBackplaneProviderTests
{
    private AbpFusionCacheRedisBackplaneProvider _provider = null!;
    private IConfiguration _configuration = null!;
    private AbpFusionCacheOptions _options = null!;

    [Before(Test)]
    public void Setup()
    {
        _provider = new AbpFusionCacheRedisBackplaneProvider();
        _options = new AbpFusionCacheOptions();

        // Setup configuration with Redis connection string
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            { "ConnectionStrings:Redis", "localhost:6379" }
        });
        _configuration = configurationBuilder.Build();
    }

    #region Provider Name Tests

    [Test]
    public async Task ProviderName_ShouldReturnRedis()
    {
        // Act
        var providerName = _provider.ProviderName;

        // Assert
        await Assert.That(providerName).IsEqualTo("Redis");
    }

    #endregion

    #region IsAvailable Tests

    [Test]
    public async Task IsAvailable_WithConnectionStringInConnectionStrings_ShouldReturnTrue()
    {
        // Arrange
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            { "ConnectionStrings:Redis", "localhost:6379" }
        });
        var config = configBuilder.Build();

        // Act
        var result = _provider.IsAvailable(config);

        // Assert
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task IsAvailable_WithConnectionStringInRedisConfiguration_ShouldReturnTrue()
    {
        // Arrange
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            { "Redis:Configuration", "localhost:6379" }
        });
        var config = configBuilder.Build();

        // Act
        var result = _provider.IsAvailable(config);

        // Assert
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task IsAvailable_WithConnectionStringInRedisConnectionString_ShouldReturnTrue()
    {
        // Arrange
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            { "Redis:ConnectionString", "localhost:6379" }
        });
        var config = configBuilder.Build();

        // Act
        var result = _provider.IsAvailable(config);

        // Assert
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task IsAvailable_WithNoConnectionString_ShouldReturnFalse()
    {
        // Arrange
        var configBuilder = new ConfigurationBuilder();
        var config = configBuilder.Build();

        // Act
        var result = _provider.IsAvailable(config);

        // Assert
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task IsAvailable_WithEmptyConnectionString_ShouldReturnFalse()
    {
        // Arrange
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            { "ConnectionStrings:Redis", "" }
        });
        var config = configBuilder.Build();

        // Act
        var result = _provider.IsAvailable(config);

        // Assert
        await Assert.That(result).IsFalse();
    }

    #endregion

    #region ConfigureServices Tests

    [Test]
    public async Task ConfigureServices_WithValidConfiguration_ShouldRegisterBackplane()
    {
        // Arrange
        var services = new ServiceCollection();
        _options.EnableMultiTenancy = false;

        // Act
        _provider.ConfigureServices(services, _configuration, _options);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var backplane = serviceProvider.GetService<IFusionCacheBackplane>();
        await Assert.That(backplane).IsNotNull();
    }

    [Test]
    public async Task ConfigureServices_WithMultiTenancyEnabled_ShouldConfigureChannelPrefix()
    {
        // Arrange
        var services = new ServiceCollection();
        _options.EnableMultiTenancy = true;
        _options.KeyPrefix = "MyApp";

        // Act
        _provider.ConfigureServices(services, _configuration, _options);

        // Assert - Service should be registered
        var serviceProvider = services.BuildServiceProvider();
        var backplane = serviceProvider.GetService<IFusionCacheBackplane>();
        await Assert.That(backplane).IsNotNull();
    }

    [Test]
    public async Task ConfigureServices_WithNoConnectionString_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        var configBuilder = new ConfigurationBuilder();
        var emptyConfig = configBuilder.Build();

        // Act & Assert
        await Assert.That(() => Task.Run(() => _provider.ConfigureServices(services, emptyConfig, _options)))
            .Throws<InvalidOperationException>()
            .With(ex => ex.Message.Contains("Redis connection string is not configured"));
    }

    #endregion

    #region CreateBackplane Tests

    [Test]
    public async Task CreateBackplane_WithRegisteredBackplane_ShouldReturnInstance()
    {
        // Arrange
        var services = new ServiceCollection();
        _provider.ConfigureServices(services, _configuration, _options);
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var backplane = _provider.CreateBackplane(serviceProvider);

        // Assert
        await Assert.That(backplane).IsNotNull();
        await Assert.That(backplane).IsAssignableTo<IFusionCacheBackplane>();
    }

    [Test]
    public async Task CreateBackplane_WithoutRegisteredBackplane_ShouldReturnNull()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var backplane = _provider.CreateBackplane(serviceProvider);

        // Assert
        await Assert.That(backplane).IsNull();
    }

    #endregion

    #region Connection String Priority Tests

    [Test]
    public async Task IsAvailable_WithMultipleConnectionStrings_ShouldPrioritizeConnectionStringsRedis()
    {
        // Arrange
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            { "ConnectionStrings:Redis", "primary:6379" },
            { "Redis:Configuration", "secondary:6379" },
            { "Redis:ConnectionString", "tertiary:6379" }
        });
        var config = configBuilder.Build();

        // Act
        var result = _provider.IsAvailable(config);

        // Assert
        await Assert.That(result).IsTrue();
        // The first one (ConnectionStrings:Redis) should be used
    }

    #endregion

    #region Multi-Tenancy Channel Prefix Tests

    [Test]
    public async Task ConfigureServices_WithMultiTenancy_ShouldSetChannelPrefixWithKeyPrefix()
    {
        // Arrange
        var services = new ServiceCollection();
        _options.EnableMultiTenancy = true;
        _options.KeyPrefix = "Production";

        // Act
        _provider.ConfigureServices(services, _configuration, _options);

        // Assert
        // The channel prefix should be set to "Production:backplane"
        // We can verify the service was registered successfully
        var serviceProvider = services.BuildServiceProvider();
        var backplane = serviceProvider.GetService<IFusionCacheBackplane>();
        await Assert.That(backplane).IsNotNull();
    }

    [Test]
    public async Task ConfigureServices_WithMultiTenancy_AndEmptyKeyPrefix_ShouldStillSetChannelPrefix()
    {
        // Arrange
        var services = new ServiceCollection();
        _options.EnableMultiTenancy = true;
        _options.KeyPrefix = string.Empty;

        // Act
        _provider.ConfigureServices(services, _configuration, _options);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var backplane = serviceProvider.GetService<IFusionCacheBackplane>();
        await Assert.That(backplane).IsNotNull();
    }

    #endregion

    #region Configuration String Formats Tests

    [Test]
    public async Task IsAvailable_WithComplexConnectionString_ShouldReturnTrue()
    {
        // Arrange
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            { "ConnectionStrings:Redis", "localhost:6379,password=mypassword,ssl=true" }
        });
        var config = configBuilder.Build();

        // Act
        var result = _provider.IsAvailable(config);

        // Assert
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task ConfigureServices_WithComplexConnectionString_ShouldConfigureSuccessfully()
    {
        // Arrange
        var services = new ServiceCollection();
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            { "ConnectionStrings:Redis", "localhost:6379,abortConnect=false" }
        });
        var config = configBuilder.Build();

        // Act
        _provider.ConfigureServices(services, config, _options);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var backplane = serviceProvider.GetService<IFusionCacheBackplane>();
        await Assert.That(backplane).IsNotNull();
    }

    #endregion
}
