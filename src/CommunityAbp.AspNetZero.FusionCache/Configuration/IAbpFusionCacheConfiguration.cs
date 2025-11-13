using CommunityAbp.AspNetZero.FusionCache.Options;

namespace CommunityAbp.AspNetZero.FusionCache.Configuration;

/// <summary>
/// Defines configuration settings and initialization logic for FusionCache integration within an ABP application.
/// </summary>
/// <remarks>Implement this interface to customize FusionCache behavior, such as setting cache options or
/// performing additional setup during application startup. Typically used by dependency injection or configuration
/// modules to ensure consistent cache configuration across the application.</remarks>
public interface IAbpFusionCacheConfiguration
{
    /// <summary>
    /// Gets the configuration options for the FusionCache integration.
    /// </summary>
    AbpFusionCacheOptions Options { get; }

    /// <summary>
    /// Configures the FusionCache integration for the application.
    /// </summary>
    void ConfigureFusionCache();
}
